using System.Net;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Contracts
{
    public interface IWebClientService
    {
        Task<WebClient> Provide();
        Task<WebClient> SetDefaultUserString(WebClient client);
        Task<WebClient> SetTls12UserString(WebClient client);
    }
}
