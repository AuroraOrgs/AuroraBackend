using Aurora.Application.Contracts;
using Aurora.Application.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Aurora.Application
{
    public class ScraperRunner : IScraperRunner
    {
        private readonly ISearchScraperCollector _collector;

        public ScraperRunner(ISearchScraperCollector collector)
        {
            _collector = collector;
        }

        public async Task<List<SearchResult>> Run(SearchRequest searchRequest, CancellationToken token = default)
        {
            var scrappers = await _collector.CollectFor(searchRequest.Websites);
            ConcurrentBag<SearchResult> resultCollection = new();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5,
                CancellationToken = token
            };

            try
            {
                Parallel.ForEach(scrappers, options, async scrapper =>
                {
                    var result = await scrapper.Search(searchRequest, token);
                    if (result.HasValue)
                    {
                        resultCollection.Add(result.Value);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                //if cancelled - do nothing
            }

            return resultCollection.ToList();
        }
    }
}