using Aurora.Application.Models;
using System;

namespace Aurora.Infrastructure.Extensions
{
    public static class SupportedWebsiteExtensions
    {
        public static string GetBaseUrl(this SupportedWebsite website) =>
            website switch
            {
                SupportedWebsite.Xvideos => "https://www.xvideos.com",
                SupportedWebsite.Pornhub => "https://www.pornhub.com",
                _ => throw new Exception($"Base url for {website} is not configured"),
            };
    }
}
