using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<bool> LoadDocumentFromUrl(this HttpClient client, HtmlDocument htmlDocument, string url)
        {
            bool reachedEnd;
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var htmlContent = await response.Content.ReadAsStringAsync();
                htmlDocument.LoadHtml(htmlContent);
                reachedEnd = false;
            }
            else
            {
                reachedEnd = true;
            }
            return reachedEnd;
        }
    }
}
