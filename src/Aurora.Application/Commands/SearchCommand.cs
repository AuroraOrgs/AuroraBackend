using Aurora.Application.Models;
using MediatR;
using System.Collections.Generic;

namespace Aurora.Application.Commands
{
    public class SearchCommand : IRequest<List<SearchResultDto>>
    {
        public SearchCommand(SearchRequestDto searchRequest)
        {
            SearchRequest = searchRequest;
            UserId = null;
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
