using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Queries;

public class GetSnapshotsQuery : IRequest<GetSnapshotsResult>
{
    public SearchRequestDto SearchRequest { get; } = null!;
    public PagingOptions? Paging { get; set; }
}
