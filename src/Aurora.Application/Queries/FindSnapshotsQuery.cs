using Aurora.Application.Models;

namespace Aurora.Application.Queries;

public class FindSnapshotsQuery : IRequest<FindSnapshotsResult>
{
    public FindSnapshotsQuery(SearchRequestDto searchRequest)
    {
        SearchRequest = searchRequest;
    }
    public SearchRequestDto SearchRequest { get; } = null!;
}
