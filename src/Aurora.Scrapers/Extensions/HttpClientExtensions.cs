namespace Aurora.Scrapers.Extensions;

public static class HttpClientExtensions
{
    public static async Task<bool> TryLoadDocumentFromUrl(this HttpClient client, HtmlDocument htmlDocument, string url)
    {
        bool succeeded;
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var htmlContent = await response.Content.ReadAsStringAsync();
            htmlDocument.LoadHtml(htmlContent);
            succeeded = true;
        }
        else
        {
            succeeded = false;
        }
        return succeeded;
    }
}
