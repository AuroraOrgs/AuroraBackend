﻿using Aurora.Application.Contracts;
using Aurora.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using Aurora.Application.Extensions;

namespace Aurora.Application.Commands
{
    public class SearchCommandHandler : IRequestHandler<SearchCommand, SearchCommandResult>
    {
        private readonly ISearchDataService _search;
        private readonly IQueueProvider _queue;
        private readonly ILogger<SearchCommandHandler> _logger;

        public SearchCommandHandler(ISearchDataService search, IQueueProvider queue, ILogger<SearchCommandHandler> logger)
        {
            _search = search;
            _queue = queue;
            _logger = logger;
        }

        public async Task<SearchCommandResult> Handle(SearchCommand requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            Action<string> log = (prefix) => _logger.LogRequest(request, prefix);
            log("Received request");

            await _search.StoreRequest(request);
            var result = await _search.GetResults(request, requestWrapper.Paging);
            List<SupportedWebsite> notCachedWebsites = request.Websites
                .Except(result.ProcessedWebsites)
                .ToList();
            log($"Found '{storedWebsites.CommaSeparate()}' already processed");
            log($"Found '{notCachedWebsites.CommaSeparate()}' not processed");

            var resultItems = result.Results;

            if (notCachedWebsites.Count > 0)
            {
                var childRequest = new SearchRequestDto
                {
                    SearchTerm = request.SearchTerm,
                    SearchOptions = new List<SearchOption>()
                    {
                        SearchOption.Image,
                        SearchOption.Video,
                        SearchOption.Gif
                    },
                    Websites = notCachedWebsites,
                    //move to config or remove
                    ResponseItemsMaxCount = 200
                };

                string websites = String.Join(", ", notCachedWebsites.Select(x => x.ToString()));
                _queue.Enqueue(
                    $"Scrapping {websites} for {request.SearchTerm}",
                    new ScrapCommand(childRequest, requestWrapper.UserId));

                foreach (var webSite in notCachedWebsites)
                {
                    resultItems.Add(new SearchResultDto(webSite));
                }
            }

            log($"Finished processing in {GetType().Name}");
            return new SearchCommandResult(resultItems, result.TotalItems);
        }
    }
}