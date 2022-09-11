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
        if (pageNumber is null && pageSize is null)
        {
            Paging = null;
        }
        else
        {
            if (pageNumber is null)
            {
                pageNumber = 0;
            }
            else
            {
                if (pageSize is null)
                {
                    pageSize = 25;
                }
            }
            Paging = new PagingOptions(pageNumber.Value!, pageSize!.Value);
        }
        UserId = userId;
    }

    public SearchRequestDto SearchRequest { get; }
    public PagingOptions? Paging { get; set; }
    public string? UserId { get; }
}
