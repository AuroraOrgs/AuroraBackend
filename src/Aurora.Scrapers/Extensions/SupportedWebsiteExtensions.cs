using Aurora.Domain.Enums;

namespace Aurora.Scrapers.Extensions;

public static class SupportedWebsiteExtensions
{
    public static string GetBaseUrl(this SupportedWebsite website) =>
        website switch
        {
            SupportedWebsite.XVideos => "https://www.xvideos.com",
            SupportedWebsite.Pornhub => "https://www.pornhub.com",
            SupportedWebsite.FootFetishBooru => "https://footfetishbooru.booru.org",
            _ => throw new Exception($"Base url for {website} is not configured"),
        };
}
