namespace Aurora.Scrapers.Behaviours
{
    public static class Scraping
    {
        public static List<SearchItem> FootFetishBooruScrap(HtmlDocument document)
        {
            var posts = document.DocumentNode.SelectNodes("//a[@id]")
                                                    .Where<HtmlNode>(x => x.Id.StartsWith("p") && x.Id != "pi");
            List<SearchItem> items = new();
            foreach (var post in posts)
            {
                var hrefValue = post.GetAttributeValue("href", "none");
                var location = $"{SupportedWebsite.FootFetishBooru.GetBaseUrl()}/{hrefValue}".Replace("&amp;", "&");
                var previewImage = post.ChildNodes.Where<HtmlNode>(x => x.Name == "img").First();
                var previewSrc = previewImage.GetAttributeValue("src", "none");

                ContentType type;
                if (previewSrc.EndsWith("gif"))
                {
                    type = ContentType.Gif;
                }
                else
                {
                    type = ContentType.Image;
                }
                var termsStr = previewImage.GetAttributeValue("title");
                SearchItem item;
                if (termsStr is not null)
                {
                    var specialTerms = new string[] { "score", "rating" };
                    var terms = termsStr.Split(" ").Select(x => x.Trim())
                        .Where(term => specialTerms.Where(special => term.StartsWith(special)).None())
                        .Where(term => term.IsNotEmpty())
                        .ToArray();
                    item = new SearchItem(type, previewSrc, location, new FootfetishBooruResultData(terms));
                }
                else
                {
                    item = new SearchItem(type, previewSrc, location);
                }
                items.Add(item);
            }
            return items;
        }
    }
}
