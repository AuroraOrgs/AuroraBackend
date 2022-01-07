using Aurora.Application.Models;
using MediatR;
using System.Collections.Generic;

namespace Aurora.Application.Commands
{
    public class SearchCommand : IRequest<List<SearchResultDto>>
    {
        public SearchCommand()
        {
            SearchRequest = new()
            {
                SearchTerm = "Haha - default is written by hand!"
            };
        }

        public SearchCommand(SearchRequestDto searchRequest, string userId)
        {
            SearchRequest = searchRequest;
            UserId = userId;
        }

        public SearchRequestDto SearchRequest { get; }
        public string? UserId { get; }
    }
}
