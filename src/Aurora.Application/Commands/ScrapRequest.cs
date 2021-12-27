using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Commands
{
    public class ScrapRequest : IRequest
    {
        public ScrapRequest(SearchRequestDto innerRequest)
        {
            SearchRequest = innerRequest;
        }
        public SearchRequestDto SearchRequest { get; set; }
    }
}
