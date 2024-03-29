﻿using Aurora.Application.Contracts;
using Aurora.Application.Models;
using System.Collections.Immutable;

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
        return new(state.StoredOptions.Select(item =>
        {
            var option = item.Key;
            var snapshots = item.Value.Snapshots;
            return new SnapshotsResult(option, snapshots);
        }).ToImmutableList());
    }
}
