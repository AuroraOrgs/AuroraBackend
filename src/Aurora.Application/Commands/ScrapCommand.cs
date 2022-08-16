using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Commands;

public class ScrapCommand : IRequest
{
    public ScrapCommand()
    {
        SearchRequest = new()
        {
            SearchTerms = new List<string>()
        };
    }

    public ScrapCommand(SearchRequestDto innerRequest, string? userId)
    {
        SearchRequest = innerRequest;
        UserId = userId;
    }

    public SearchRequestDto SearchRequest { get; }
    public string? UserId { get; }
}
