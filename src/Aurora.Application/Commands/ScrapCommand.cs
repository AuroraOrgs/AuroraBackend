using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Commands
{
    public class ScrapCommand : IRequest
    {
        public ScrapCommand(SearchRequestDto innerRequest)
        {
            SearchRequest = innerRequest;
        }
        public SearchRequestDto SearchRequest { get; set; }
    }
}
