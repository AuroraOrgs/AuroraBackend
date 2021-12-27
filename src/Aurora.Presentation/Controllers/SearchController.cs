using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Models;
using System.Threading;
using Aurora.Application;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IScraperRunner _runner;

        public SearchController(IScraperRunner runner)
        {
            _runner = runner;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto searchRequest, CancellationToken token)
        {
            var result = await _runner.Run(searchRequest, token);
            return Ok(result);
        }
    }
}