using Aurora.Application.Models;

namespace Aurora.Application.Commands;

public class SearchCommand : IRequest<SearchCommandResult>
{
    public SearchCommand()
    {
        SearchRequest = new()
        {
            SearchTerms = new List<string>()
        };
    }

    public SearchCommand(SearchRequestDto searchRequest, int? pageNumber, int? pageSize, string? userId)
    {
        SearchRequest = searchRequest;
        Paging = PagingOptions.Create(pageNumber, pageSize);
        UserId = userId;
    }

    public SearchRequestDto SearchRequest { get; }
    public PagingOptions? Paging { get; set; }
    public string? UserId { get; }
}
