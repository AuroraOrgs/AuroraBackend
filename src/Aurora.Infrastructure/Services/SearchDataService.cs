using Aurora.Application.Contracts;
using Aurora.Application.Entities;
using Aurora.Application.Extensions;
using Aurora.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class SearchDataService : ISearchDataService
    {
        private readonly SearchContext _context;
        private readonly ILogger<SearchDataService> _logger;

        public SearchDataService(SearchContext context, ILogger<SearchDataService> logger)
        {
            _context = context;
            _logger = logger;
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
                        _logger.LogInformation(
                            "Creating request with '{0}' option, '{1}' website and '{2} term'",
                            option, webSite, request.SearchTerm);
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
                            .Where(x => request.SearchOptions.Contains(x.ContentOption)
                                && request.Websites.Contains(x.Website)
                                && request.SearchTerm == x.SearchTerm)
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
                        {
                            var newRequest = new SearchRequest()
                            {
                                SearchTerm = request.SearchTerm,
                                ContentOption = option,
                                OccurredCount = 0,
                                Website = website
                            };
                            _logger.LogRequest(newRequest, "Creating new option ");
                            await _context.Request.AddAsync(newRequest);
                            //id is set by ef
                            requestId = newRequest.Id;
                            optionsToId[(website, option)] = requestId;
                        }

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

            _logger.LogInformation("Removing statle search results");
            var removedRows = await _context.Result
                .Where(x => existingIds.Contains(x.RequestId))
                .DeleteFromQueryAsync();
            _logger.LogInformation("Removed '{0}' rows of stale search results", removedRows);
            await _context.Result
                .AddRangeAsync(searchResults);
            _logger.LogInformation("Stored '{0}' rows of new search results", searchResults.Count);

            await _context.SaveChangesAsync();
        }

        public async Task<SearchResults> GetResults(SearchRequestDto request, PagingOptions? paging)
        {
            var filteredResults = _context.Result
                .Include(x => x.Request)
                .Where(x => request.SearchTerm == x.Request.SearchTerm
                    && request.SearchOptions.Contains(x.Request.ContentOption)
                    && request.Websites.Contains(x.Request.Website));
            var query = filteredResults;
            if (paging is not null)
            {
                var toSkip = paging.PageSize * paging.PageNumber;
                query = query
                    .OrderBy(x => x.Id)
                    .Skip(toSkip)
                    .Take(paging.PageSize);
            }
            var storedResults = await query
                .ToListAsync();

            var results = Convert(storedResults);
            var count = await filteredResults.CountAsync();
            var websites = await filteredResults.Select(x => x.Request.Website)
                                                .Distinct()
                                                .ToListAsync();
            _logger.LogRequest(request, $"Loaded '{storedResults.Count}' existing results");
            return new SearchResults(results, count, websites);
        }

        private static List<SearchResultDto> Convert(List<SearchResult> results)
        {
            return results.GroupBy(result => result.Request.Website)
                .Select(resultsByWebsite => new SearchResultDto(ConvertResults(resultsByWebsite), resultsByWebsite.Key))
                .ToList();
        }

        private static List<SearchItem> ConvertResults(IEnumerable<SearchResult> results)
        {
            return results
                .Select(result => new SearchItem(result.Request.ContentOption, result.SearchItemUrl, result.ImagePreviewUrl))
                .ToList();
        }
    }
}
