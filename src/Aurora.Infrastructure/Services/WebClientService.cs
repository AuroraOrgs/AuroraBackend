using Aurora.Infrastructure.Contracts;
using System.Net;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class WebClientService : IWebClientService
    {
        private readonly WebClient _httpClient;

        public WebClientService()
        {
            // WebRequest, HttpWebRequest, ServicePoint, WebClient are obsolete.
            // Use HttpClient.
            _httpClient = new WebClient();
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return _httpClient.Headers;
            }
            set
            {
                _httpClient.Headers = value;
            }
        }

        public Task<WebClient> Provide()
        {
            return Task.FromResult(_httpClient);
        }
    }
}
