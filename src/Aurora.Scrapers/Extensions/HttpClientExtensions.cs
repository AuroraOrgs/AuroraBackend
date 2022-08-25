namespace Aurora.Scrapers.Extensions;

public static class HttpClientExtensions
{
    public static async Task<ValueOrNull<HtmlDocument>> TryLoadDocumentFromUrl(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        ValueOrNull<HtmlDocument> result;
        if (response.IsSuccessStatusCode)
        {
            var document = new HtmlDocument()
            {
                OptionFixNestedTags = true
            };
            var htmlContent = await response.Content.ReadAsStringAsync();
            document.LoadHtml(htmlContent);
            result = document;
        }
        else
        {
            result = $"Failed to load page from '{url}'".ToErrorResult<HtmlDocument>();
        }
        return result;
    }
}
