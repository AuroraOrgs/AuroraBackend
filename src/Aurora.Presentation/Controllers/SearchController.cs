using Aurora.Application.Commands;
using Aurora.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Search(
            [FromBody] SearchRequestDto searchRequest,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            CancellationToken token)
        {
            SearchCommand command;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim is not null)
            {
                var id = idClaim.Value;
                command = new SearchCommand(searchRequest, pageNumber, pageSize, id);
            }
            else
            {
                command = new SearchCommand(searchRequest, pageNumber, pageSize, null);
            }
            var result = await _mediator.Send(command, token);
            return Ok(result);
        }
    }
}