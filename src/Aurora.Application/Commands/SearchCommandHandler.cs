using Aurora.Application.Contracts;
using Aurora.Application.Enums;
using Aurora.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Commands
{
    public class SearchCommandHandler : IRequestHandler<SearchCommand, List<SearchResultDto>>
    {
        private readonly ISearchDataService _search;
        private readonly IQueueProvider _queue;

        public SearchCommandHandler(ISearchDataService search, IQueueProvider queue)
        {
            _search = search;
            _queue = queue;
        }

        public async Task<List<SearchResultDto>> Handle(SearchCommand requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.SearchRequest;
            await _search.StoreRequest(request);
            var results = await _search.GetResults(request);
            var storedWebsite = results.Select(x => x.Website).ToList();
            List<SupportedWebsite> notCachedWebsites = request.Websites
                .Where(x => storedWebsite.Contains(x) == false)
                .ToList();

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

                _queue.Enqueue(new ScrapRequest(childRequest));

                foreach (var webSite in notCachedWebsites)
                {
                    results.Add(new SearchResultDto(webSite));
                }
            }

            return results;
        }
    }
}