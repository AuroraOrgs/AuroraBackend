﻿using Aurora.Application.Contracts;
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

            foreach (var webSite in request.Websites)
            {
                foreach (var option in request.SearchOptions)
                {
                    if (existingRequests.Where(x => x.Website == webSite && x.ContentOption == option).Any() == false)
                    {
                        //TODO: Bulk insert
                        var inserted = new SearchRequest()
                        {
                            ContentOption = option,
                            OccurredCount = 1,
                            SearchTerm = request.SearchTerm,
                            Website = webSite
                        };
                        await _context.Request
                            .AddAsync(inserted);
                    }
                }
            }

            foreach (var existingRequest in existingRequests)
            {
                existingRequest.OccurredCount++;
            }

            _context.Request.UpdateRange(existingRequests);

            await _context.SaveChangesAsync();
        }

        private async Task<List<SearchRequest>> GetExistingFor(SearchRequestDto request)
        {
            return await _context.Request
                            .Where(x => request.SearchOptions.Contains(x.ContentOption) && request.Websites.Contains(x.Website))
                            .ToListAsync();
        }

        public async Task AddOrUpdateResults(SearchRequestDto request, IEnumerable<SearchResultDto> resultDtos)
        {
            var webSiteToGroupToResult = resultDtos.GroupBy(x => Enum.Parse<SupportedWebsite>(x.Website))
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
                        var searchResult = new SearchResult()
                        {
                            ImagePreviewUrl = result.ImagePreviewUrl,
                            SearchItemUrl = result.SearchItemUrl,
                            RequestId = optionsToId[(website, option)]
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
                .Where(x => request.SearchOptions.Contains(x.Request.ContentOption) && request.Websites.Contains(x.Request.Website))
                .ToListAsync();

            return Convert(storedResults); 
        }

        private static List<SearchResultDto> Convert(List<SearchResult> results)
        {
            return results.GroupBy(x => x.Request.Website)
                .Select(x => (x.Key, x.Select(y => new SearchItem()
                {
                    Option = y.Request.ContentOption,
                    SearchItemUrl = y.SearchItemUrl,
                    ImagePreviewUrl = y.ImagePreviewUrl
                })))
                .Select(x => new SearchResultDto(x.Item2.ToList(), x.Key.ToString()))
                .ToList();
        }
    }
}