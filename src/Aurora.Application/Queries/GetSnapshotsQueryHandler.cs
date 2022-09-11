using Aurora.Application.Contracts;
using Aurora.Application.Models;
using System.Collections.Immutable;

namespace Aurora.Application.Queries;

public class GetSnapshotsQueryHandler : IRequestHandler<GetSnapshotsQuery, GetSnapshostResult>
{
    private readonly ISearchQueryService _query;

    public GetSnapshotsQueryHandler(ISearchQueryService query)
    {
        _query = query;
    }

    public async Task<GetSnapshostResult> Handle(GetSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var results = await _query.GetResults(request.SnapshotIds, request.Paging);
        return new(results.Results.ToImmutableList());
    }
}
