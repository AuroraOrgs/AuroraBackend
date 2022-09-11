using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Queries;

public class FindSnapshotsQuery : IRequest<FindSnapshotsResult>
{
    public SearchRequestDto SearchRequest { get; } = null!;
    public PagingOptions? Paging { get; set; }
}
