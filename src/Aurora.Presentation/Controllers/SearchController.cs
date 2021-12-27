using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Models;
using System.Threading;
using Aurora.Application;
using Aurora.Application.Contracts;
using Aurora.Application.Commands;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IScraperRunner _runner;
        private readonly IQueueProvider _queue;

        public SearchController(IScraperRunner runner, IQueueProvider queue)
        {
            _runner = runner;
            _queue = queue;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto searchRequest, CancellationToken token)
        {
            _queue.Enqueue("Scrapping", new ScrapRequest()
            {
                SearchRequest = searchRequest
            });
            //var result = await _runner.Run(searchRequest, token);
            return Ok(null);
        }
    }
}