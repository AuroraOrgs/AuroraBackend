using Aurora.Application.Contracts;
using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Queries;

public class GetSnapshotsQueryHandler : IRequestHandler<GetSnapshotsQuery, GetSnapshotsResult>
{
    private readonly ISearchRepository _search;

    public GetSnapshotsQueryHandler(ISearchRepository search)
    {
        _search = search;
    }

    public async Task<GetSnapshotsResult> Handle(GetSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var state = await _search.FetchRequest(request.SearchRequest, true);
        return new(state.StoredOptions.ToDictionary(x => x.Key, x => x.Value.Snapshots));
    }
}
