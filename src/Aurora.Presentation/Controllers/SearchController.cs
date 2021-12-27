using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Models;
using System.Threading;
using Aurora.Application;
using Aurora.Application.Contracts;
using Aurora.Application.Commands;
using MediatR;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto searchRequest, CancellationToken token)
        {
            var result = await _mediator.Send(new SearchCommand(searchRequest));
            return Ok(result);
        }
    }
}