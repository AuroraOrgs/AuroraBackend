using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Contracts;
using Aurora.Application.Models;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchScraperCollector _searchScraperCollector;

        public SearchController(ISearchScraperCollector searchScraperCollector)
        {
            _searchScraperCollector = searchScraperCollector;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromBody] SearchRequest searchRequest)
        {
            var scrappers = await _searchScraperCollector.Collect(searchRequest.Websites);
            List<SearchResult> resultCollection = new();
            
            // Parallel.ForEach(scrappers, async scrapper =>
            // {
            //     var result = await scrapper.Search(searchRequest);
            //     resultCollection.Add(result);
            // });

            foreach (var scrapper in scrappers)
            {
                var result = await scrapper.Search(searchRequest);
                resultCollection.Add(result);
            }
            
            return Ok(resultCollection);
        }
    }
}