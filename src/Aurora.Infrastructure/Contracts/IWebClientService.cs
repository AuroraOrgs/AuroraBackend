using System.Net;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Contracts
{
    public interface IWebClientService
    {
        Task<WebClient> Provide();
        Task<WebClient> SetUserString(WebClient client);
    }
}
