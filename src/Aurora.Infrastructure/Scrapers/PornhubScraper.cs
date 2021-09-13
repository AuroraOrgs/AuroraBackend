using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Contracts;
using Aurora.Shared.Models;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubScraper : ScraperBase
    {
        private const int PAGE_NUMBER_LIMIT = 5;
        private const string _baseUrl = "https://www.pornhub.com";
        private const string _dateSourcePhncdn = "https://dl.phncdn.com";
        private readonly IWebClientService _clientProvider;

        public PornhubScraper(ILogger<ISearchScraper> logger, IWebClientService clientProvider) : base(logger)
        {
            _clientProvider = clientProvider;
        }

        public override async Task<ValueOrNull<SearchResult>> SearchInner(
            SearchRequest request,
            CancellationToken token = default)
        {
            SearchResult result = new()
            {
                Website = _baseUrl
            };

            try
            {
                if (request.SearchOptions.Contains(SearchOption.Video))
                {
                    var videos = await ScrapVideos(request.SearchTerm, request.ResponseItemsMaxCount);
                    result.Items.AddRange(videos);
                }

                if (request.SearchOptions.Contains(SearchOption.Gif))
                {
                    var gifs = await ScrapGifs(request.SearchTerm, request.ResponseItemsMaxCount);
                    result.Items.AddRange(gifs);
                }

                if (request.SearchOptions.Contains(SearchOption.Image))
                {
                    var images = await ScrapImages(request.SearchTerm, request.ResponseWebsitesMaxCount);
                    result.Items.AddRange(images);
                }
            }
            catch (Exception exception)
            {
                return ValueOrNull<SearchResult>.CreateNull(exception.Message);
            }

            result.CountItems = result.Items.Count;
            return ValueOrNull<SearchResult>.CreateValue(result);
        }

        private async Task<IEnumerable<SearchItem>> ScrapVideos(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;

            var urlsCount = 0;

            using (var client = await _clientProvider.Provide())
            {
                for (int i = 0; i < PAGE_NUMBER_LIMIT; i++)
                {
                    if (urlsCount >= maxNumberOfVideoUrls)
                    {
                        break;
                    }

                    // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                    var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                    var searchPageUrl = $"{_baseUrl}/video/search?search={searchTermUrlFormatted}&page={pageNumber}";
                    await _clientProvider.SetUserString(client);
                    var htmlSearchPage = client.DownloadString(searchPageUrl);
                    htmlDocument.LoadHtml(htmlSearchPage);

                    var videoLinksNodes = htmlDocument.DocumentNode
                        ?.SelectSingleNode("//body")
                        ?.SelectNodes("//a[contains(@class, 'videoPreviewBg')]");

                    if (videoLinksNodes is null)
                    {
                        break;
                    }

                    foreach (var videoLinkNode in videoLinksNodes)
                    {
                        SearchItem searchVideoItem = new()
                        {
                            Option = SearchOption.Video
                        };

                        var currentLinkImageNode = videoLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "img");

                        if (currentLinkImageNode != null)
                        {
                            var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                            searchVideoItem.ImagePreviewUrl = currentLinkImageAttributes["data-thumb_url"].Value;
                        }

                        var currentLinkAttributes = videoLinkNode.Attributes;
                        searchVideoItem.SearchItemUrl = $"{_baseUrl}{currentLinkAttributes["href"].Value}";

                        urlsCount++;
                        videoItems.Add(searchVideoItem);
                    }

                    pageNumber++;
                    await Task.Delay(1000);
                }
            }

            return videoItems;
        }

        private async Task<IEnumerable<SearchItem>> ScrapImages(string requestSearchTerm, int requestResponseWebsitesMaxCount)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var searchTerm = FormatTermToUrl(requestSearchTerm);
            List<SearchItem> result = new List<SearchItem>();
            using (var client = await _clientProvider.Provide())
            {
                for (int i = 0; i < PAGE_NUMBER_LIMIT; i++)
                {
                    var pageNumber = i + 1;
                    var fullUrl = GetImagePageUrl(searchTerm, pageNumber);
                    await _clientProvider.SetUserString(client);
                    var html = client.DownloadString(fullUrl);
                    htmlDocument.LoadHtml(html);

                    var albumListNode = htmlDocument.DocumentNode
                        ?.SelectSingleNode("//*[@id='photosAlbumsSection']");
                    var albumNodes = albumListNode?.SelectNodes("//li[contains(@class,'photoAlbumListContainer')]/div/a");

                    if (albumNodes.IsNotNull())
                    {
                        foreach (var albumNode in albumNodes)
                        {
                            var linkUrl = albumNode.Attributes["href"]?.Value;
                            if (linkUrl.IsNotNull())
                            {
                                await _clientProvider.SetUserString(client);
                                string albumUrl = $"{_baseUrl}{linkUrl}";
                                var albumHtml = client.DownloadString(albumUrl);
                                htmlDocument.LoadHtml(albumHtml);

                                var images = htmlDocument.DocumentNode
                                    ?.SelectSingleNode("//ul[contains(@class, 'photosAlbumsListing')]")
                                    ?.SelectNodes("//li/div");

                                if (images.IsNotNull())
                                {
                                    foreach (var image in images)
                                    {
                                        var preview = image.Attributes["data-bkg"].Value;
                                        var url = image.SelectSingleNode("/a").Attributes["href"].Value;
                                        var item = new SearchItem
                                        {
                                            ImagePreviewUrl = preview,
                                            SearchItemUrl = url,
                                            Option = SearchOption.Image
                                        };
                                        result.Add(item);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return result;
        }

        private string GetImagePageUrl(string searchTerm, int pageNumber)
        {
            searchTerm = FormatTermToUrl(searchTerm);
            return $"https://rt.pornhub.com/albums?search={searchTerm}&page={pageNumber}";
        }

        private async Task<IEnumerable<SearchItem>> ScrapGifs(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> gifItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;
            const int pageNumberLimit = 5;

            var urlsCount = 0;

            using (var client = await _clientProvider.Provide())
            {
                for (int i = 0; i < pageNumberLimit; i++)
                {
                    if (urlsCount >= maxNumberOfVideoUrls)
                    {
                        break;
                    }

                    // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
                    var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                    var searchPageUrl = $"{_baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={pageNumber}";
                    await _clientProvider.SetUserString(client);
                    var htmlSearchPage = client.DownloadString(searchPageUrl);
                    htmlDocument.LoadHtml(htmlSearchPage);

                    var gifLinksNodes = htmlDocument.DocumentNode
                        ?.SelectSingleNode("//body")
                        ?.SelectNodes("//li[contains(@class, 'gifVideoBlock')]");

                    if (gifLinksNodes is null || pageNumber == pageNumberLimit)
                    {
                        break;
                    }

                    foreach (var gifLinkNode in gifLinksNodes)
                    {
                        SearchItem searchVideoItem = new()
                        {
                            Option = SearchOption.Gif
                        };

                        var currentLinkGifNode = gifLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "a");

                        if (currentLinkGifNode != null)
                        {
                            var currentLinkImageAttributes = currentLinkGifNode.Attributes;
                            searchVideoItem.ImagePreviewUrl = $"{_baseUrl}{currentLinkImageAttributes["href"].Value}";
                            searchVideoItem.SearchItemUrl = $"{_dateSourcePhncdn}{currentLinkImageAttributes["href"].Value}.gif";
                        }

                        urlsCount++;
                        gifItems.Add(searchVideoItem);
                    }

                    pageNumber++;
                    await Task.Delay(1000);
                }
            }

            return gifItems;
        }

        private static string FormatTermToUrl(string term)
        {
            return term.Replace(" ", "+");
        }
    }
}