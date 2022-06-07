using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Models;
using System.Threading;
using Aurora.Application.Commands;
using MediatR;
using System.Security.Claims;
using System;
using Aurora.Application.Enums;
using System.Collections.Generic;
using System.Linq;

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
                command = new SearchCommand(searchRequest, pageNumber, pageSize, null!);
            }
            var result = await _mediator.Send(command, token);
            return Ok(result);
        }

        [HttpGet("supported-websites")]
        public IActionResult SupportedWebsites()
        {
            return Ok(EnumValueToName<SupportedWebsite>());
        }

        [HttpGet("supported-content-types")]
        public IActionResult SupportedContentTypes()
        {
            return Ok(EnumValueToName<SearchOption>());
        }

        private Dictionary<int, string> EnumValueToName<T>() where T : struct, Enum
        {
            var vals = Enum.GetValues<T>();
            return vals.ToDictionary(x => Int32.Parse(x.ToString("D")), y => y.ToString());
        }
    }
}