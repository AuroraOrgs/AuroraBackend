using System.Net;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Contracts
{
    public interface IWebClientService
    {
        Task<WebClient> Provide();
        WebHeaderCollection Headers { get; set; }
    }
}
