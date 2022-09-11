using Aurora.Application.Models;

namespace Aurora.Application.Queries;

public class FindSnapshotsQuery : IRequest<FindSnapshotsResult>
{
    public SearchRequestDto SearchRequest { get; } = null!;
    public PagingOptions? Paging { get; set; }
}
