using Aurora.Application.Contracts;
using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Queries;

public class FindSnapshotsQueryHandler : IRequestHandler<FindSnapshotsQuery, FindSnapshotsResult>
{
    private readonly ISearchRepository _search;

    public FindSnapshotsQueryHandler(ISearchRepository search)
    {
        _search = search;
    }

    public async Task<FindSnapshotsResult> Handle(FindSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var state = await _search.FetchRequest(request.SearchRequest, true);
        return new(state.StoredOptions.ToDictionary(x => x.Key, x => x.Value.Snapshots));
    }
}
