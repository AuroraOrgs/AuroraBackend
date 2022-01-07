﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Aurora.Application.Models;
using System.Threading;
using Aurora.Application.Commands;
using MediatR;
using System.Security.Claims;
using System;
using Aurora.Application.Enums;

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
            SearchCommand command;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim is not null)
            {
                var id = idClaim.Value;
                command = new SearchCommand(searchRequest, id);
            }
            else
            {
                command = new SearchCommand(searchRequest, null!);
            }
            var result = await _mediator.Send(command, token);
            return Ok(result);
        }

        [HttpGet("supported-websites")]
        public IActionResult SupportedWebsites()
        {
            return Ok(Enum.GetNames(typeof(SupportedWebsite)));
        }
    }
}