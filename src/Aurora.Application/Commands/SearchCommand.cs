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
        }
        public SearchRequestDto SearchRequest { get; set; }
    }
}
