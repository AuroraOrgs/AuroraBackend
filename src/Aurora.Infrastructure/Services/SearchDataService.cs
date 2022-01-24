using Aurora.Application.Contracts;
using Aurora.Application.Entities;
using Aurora.Application.Enums;
using Aurora.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class SearchDataService : ISearchDataService
    {
        private readonly SearchContext _context;

        public SearchDataService(SearchContext context)
        {
            _context = context;
        }

        public async Task StoreRequest(SearchRequestDto request)
        {
            List<SearchRequest> existingRequests = await GetExistingFor(request);

            List<SearchRequest> newRequests = new List<SearchRequest>();
            foreach (var webSite in request.Websites)
            {
                foreach (var option in request.SearchOptions)
                {
                    if (SearchOptionNotCreated(existingRequests, webSite, option))
                    {
                        var newRequest = new SearchRequest()
                        {
                            ContentOption = option,
                            OccurredCount = 1,
                            SearchTerm = request.SearchTerm,
                            Website = webSite
                        };
                        newRequests.Add(newRequest);
                    }
                }
            }

            await _context.Request
                .AddRangeAsync(newRequests);

            foreach (var existingRequest in existingRequests)
            {
                existingRequest.OccurredCount++;
            }

            _context.Request.UpdateRange(existingRequests);

            await _context.SaveChangesAsync();
        }

        private static bool SearchOptionNotCreated(List<SearchRequest> existingRequests, SupportedWebsite webSite, SearchOption option)
        {
            return existingRequests.Where(x => x.Website == webSite && x.ContentOption == option).Any() == false;
        }

        private async Task<List<SearchRequest>> GetExistingFor(SearchRequestDto request)
        {
            return await _context.Request
                            .Where(x => request.SearchOptions.Contains(x.ContentOption) && request.Websites.Contains(x.Website))
                            .ToListAsync();
        }

        public async Task AddOrUpdateResults(SearchRequestDto request, IEnumerable<SearchResultDto> resultDtos)
        {
            var webSiteToGroupToResult = resultDtos.GroupBy(x => x.Website)
                .Select(x =>
                (WebSite: x.Key, x.SelectMany(x => x.Items)
                                    .GroupBy(x => x.Option)
                                    .Select(x => (Option: x.Key, Results: x.ToList()))));

            var existing = await GetExistingFor(request);

            var optionsToId = existing.ToDictionary(x => (x.Website, x.ContentOption), x => x.Id);

            var searchResults = new List<SearchResult>();
            foreach (var (website, optionToResults) in webSiteToGroupToResult)
            {
                foreach (var (option, results) in optionToResults)
                {
                    foreach (var result in results)
                    {
                        var hasOptionsToIdRequestId = optionsToId.TryGetValue((website, option), out var requestId);

                        if (hasOptionsToIdRequestId == false)
                            break;

                        var searchResult = new SearchResult()
                        {
                            ImagePreviewUrl = result.ImagePreviewUrl,
                            SearchItemUrl = result.SearchItemUrl,
                            RequestId = requestId
                        };
                        searchResults.Add(searchResult);
                    }
                }
            }
            var existingIds = optionsToId.Values;
            await _context.Result
                .Where(x => existingIds.Contains(x.RequestId))
                .DeleteFromQueryAsync();
            await _context.Result
                .AddRangeAsync(searchResults);

            await _context.SaveChangesAsync();
        }

        public async Task<List<SearchResultDto>> GetResults(SearchRequestDto request)
        {
            var storedResults = await _context.Result
                .Include(x => x.Request)
                .Where(x => request.SearchTerm == request.SearchTerm 
                    && request.SearchOptions.Contains(x.Request.ContentOption)
                    && request.Websites.Contains(x.Request.Website))
                .ToListAsync();

            return Convert(storedResults);
        }

        private static List<SearchResultDto> Convert(List<SearchResult> results)
        {
            return results.GroupBy(x => x.Request.Website)
                .Select(x => (x.Key, x.Select(y => new SearchItem(y.Request.ContentOption, y.SearchItemUrl, y.ImagePreviewUrl))))
                .Select(x => new SearchResultDto(x.Item2.ToList(), x.Key))
                .ToList();
        }
    }
}
