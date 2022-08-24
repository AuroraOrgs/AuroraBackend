namespace Aurora.Scrapers.Behaviours
{
    /// <summary>
    /// This is a class for scraping behaviour that is shared between multiple scrapers
    /// </summary>
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
                    FootfetishBooruResultData resultData = ExtractDataFrom(termsStr);
                    item = new SearchItem(type, previewSrc, location, resultData);
                }
                else
                {
                    item = new SearchItem(type, previewSrc, location);
                }
                items.Add(item);
            }
            return items;
        }

        private static FootfetishBooruResultData ExtractDataFrom(string termsStr)
        {
            var titleTerms = termsStr.Split(" ").Select(x => x.Trim());
            List<string> tags = new List<string>();
            int score = 0;
            string rating = "";
            foreach (var termPart in titleTerms)
            {
                if (termPart.StartsWith("score:"))
                {
                    var scoreParts = termPart.Split(":");
                    if (scoreParts.Length == 2)
                    {
                        Int32.TryParse(scoreParts[1], out int scoreRating);
                    }
                }
                else
                {
                    if (termPart.StartsWith("rating:"))
                    {
                        var scoreParts = termPart.Split(":");
                        if (scoreParts.Length == 2)
                        {
                            rating = scoreParts[1];
                        }
                    }
                    else
                    {
                        tags.Add(termPart);
                    }
                }
            }
            return new FootfetishBooruResultData(tags.ToArray(), score, rating);
        }

        public static ValueOrNull<int> ExtractFootfetishBooruPagesCount(HtmlDocument searchPage)
        {
            ValueOrNull<int> result;
            var paginator = searchPage.DocumentNode.SelectSingleNode("//div[@id='paginator']");
            if (paginator is not null)
            {
                var lastButton = paginator.ChildNodes.Last();
                if (lastButton is not null && lastButton.GetAttributeValue("alt", "none") == "last page")
                {
                    const string defVal = "none";
                    var lastButtonReference = lastButton.GetAttributeValue("href", defVal);
                    var pidPart = lastButtonReference.Split("&amp;").Where(x => x.StartsWith("pid")).FirstOrDefault();
                    var lastPidStr = pidPart?.Split('=')?.LastOrDefault();
                    if (lastPidStr is not null && int.TryParse(lastPidStr, out int lastPid))
                    {
                        result = lastPid / ScraperConstants.FootFetishBooruPostsPerPage + 1;
                    }
                    else
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = ValueOrNull<int>.CreateNull();
                }
            }
            else
            {
                result = ValueOrNull<int>.CreateNull();
            }
            return result;
        }

    }
}
