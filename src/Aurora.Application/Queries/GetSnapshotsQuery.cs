using Aurora.Application.Models;
using System.Collections.Immutable;

namespace Aurora.Application.Queries;

public class GetSnapshotsQuery : IRequest<GetSnapshostResult>
{
    public GetSnapshotsQuery(ImmutableList<Guid> snapshots, PagingOptions paging)
    {
        SnapshotIds = snapshots;
        Paging = paging;
    }


    public GetSnapshotsQuery(ImmutableList<Guid> snapshots)
    {
        SnapshotIds = snapshots;
    }

    public ImmutableList<Guid> SnapshotIds { get; set; }
    public PagingOptions? Paging { get; set; }
}
