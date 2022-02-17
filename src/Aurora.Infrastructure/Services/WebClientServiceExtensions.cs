using Aurora.Infrastructure.Contracts;
using System.Net;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public static class WebClientServiceExtensions
    {
        public static Task<IWebClientService> SetDefaultUserString(this IWebClientService client)
        {
            client = ConfigureSecurityAccessToWebsite(client,
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)", SecurityProtocolType.SystemDefault);
            return Task.FromResult(client);
        }

        public static Task<IWebClientService> SetTls12UserString(this IWebClientService client)
        {
            client = ConfigureSecurityAccessToWebsite(client,
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36 OPR/79.0.4143.72",
                SecurityProtocolType.Tls12);

            return Task.FromResult(client);
        }

        private static IWebClientService ConfigureSecurityAccessToWebsite(this IWebClientService client, string userAgent, SecurityProtocolType protocol)
        {
            client.Headers.Add("user-agent", userAgent);
            ServicePointManager.SecurityProtocol = protocol;

            return client;
        }
    }
}
