using Aurora.Application.Models;
using MediatR;

namespace Aurora.Application.Commands
{
    public class ScrapRequest : IRequest
    {
        public SearchRequestDto SearchRequest { get; set; }
    }
}
