using Aurora.Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class WebClientService : IWebClientService
    {
        public Task<WebClient> Provide()
        {
            WebClient client = new WebClient();
            return Task.FromResult(client);
        }

        public Task<WebClient> SetUserString(WebClient client)
        {
            client = ConfigureSecurityAccessToWebsite(client);
            return Task.FromResult(client);
        }

        private static WebClient ConfigureSecurityAccessToWebsite(WebClient client)
        {
            client.Headers.Add("user-agent",
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            return client;
        }

    }
}
