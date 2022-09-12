using Aurora.Application.Models;
using Aurora.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace Aurora.Presentation.Controllers;

[Route("api/v1/snapshots")]
[ApiController]
public class SnapshotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SnapshotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    //marking this as post is needed to allow us to attach a body to it
    //we do not actually write anything as part of this request
    [HttpPost("find")]
    public async Task<IActionResult> FindSnapshots(
         [FromBody] SearchRequestDto searchRequest,
         CancellationToken token)
    {
        var query = new FindSnapshotsQuery(searchRequest);
        var response = await _mediator.Send(query, token);
        return Ok(response);
    }

    [HttpGet(template: "load")]
    public async Task<IActionResult> LoadSnapshots(
         [FromQuery] List<Guid> snapshots,
         [FromQuery] int? pageNumber,
         [FromQuery] int? pageSize,
         CancellationToken token)
    {
        var snapshotIds = snapshots.ToImmutableList();
        var paging = PagingOptions.Create(pageNumber, pageSize);

        var query = paging is not null
            ? new GetSnapshotsQuery(snapshotIds, paging)
            : new GetSnapshotsQuery(snapshotIds);
        var response = await _mediator.Send(query, token);
        return Ok(response);
    }
}
