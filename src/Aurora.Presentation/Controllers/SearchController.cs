using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Contracts;
using Aurora.Application.Models;
using System.Threading;
using System;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchScraperCollector _searchScraperCollector;

        public SearchController(ISearchScraperCollector searchScraperCollector)
        {
            _searchScraperCollector = searchScraperCollector;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest searchRequest, CancellationToken token)
        {
            var scrappers = await _searchScraperCollector.Collect(searchRequest.Websites);
            List<SearchResult> resultCollection = new();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5,
                CancellationToken = token
            };

            try
            {
                Parallel.ForEach(scrappers, options, async scrapper =>
                {
                    var result = await scrapper.Search(searchRequest);
                    resultCollection.Add(result);
                });
            }
            catch (OperationCanceledException)
            {
                //if cancelled - do nothing
            }

            return Ok(resultCollection);
        }
    }
}