using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Models;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;
using Aurora.Infrastructure.Services;
using Aurora.Application.Enums;
using System.Net.Http;
using Aurora.Application;
using System.Net.Http.Json;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubScraper : ScraperBase
    {
        private const int PAGE_NUMBER_LIMIT = 5;
        private const string _dateSourcePhncdn = "https://dl.phncdn.com";
        private readonly IHttpClientFactory _clientProvider;
        private readonly DriverInitializer _initializer;

        public override SupportedWebsite WebSite => SupportedWebsite.Pornhub;

        public PornhubScraper(ILogger<PornhubScraper> logger, IHttpClientFactory clientProvider,
            DriverInitializer initializer) : base(logger)
        {
            _clientProvider = clientProvider;
            _initializer = initializer;
        }

        public override async Task<ValueOrNull<List<SearchItem>>> SearchVideosInner(
            SearchRequestDto request,
            CancellationToken token = default)
        {
            return await ScrapVideos(request.SearchTerm, request.ResponseItemsMaxCount);
        }

        public override async Task<ValueOrNull<List<SearchItem>>> SearchGifsInner(
            SearchRequestDto request,
            CancellationToken token = default)
        {
            return await ScrapGifs(request.SearchTerm, request.ResponseItemsMaxCount);
        }

        public override async Task<ValueOrNull<List<SearchItem>>> SearchImagesInner(
            SearchRequestDto request,
            CancellationToken token = default)
        {
            return await ScrapImages(request.SearchTerm, request.ResponseItemsMaxCount);
        }

        private async Task<List<SearchItem>> ScrapVideos(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientConstants.PornhubClient);
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"/video/search?search={searchTermUrlFormatted}&page={pageNumber}";

                var htmlSearchPage = await client.GetStringAsync(searchPageUrl);
                htmlDocument.LoadHtml(htmlSearchPage);

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode
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

                    if (currentLinkImageNode is not null)
                    {
                        var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                        searchVideoItem.ImagePreviewUrl = currentLinkImageAttributes["data-thumb_url"].Value;
                    }

                    var currentLinkAttributes = videoLinkNode.Attributes;
                    searchVideoItem.SearchItemUrl = $"{client.BaseAddress}{currentLinkAttributes["href"].Value}";

                    urlsCount++;
                    videoItems.Add(searchVideoItem);
                }

                pageNumber++;
                await Task.Delay(1000);
            }

            return videoItems;
        }

        private async Task<List<SearchItem>> ScrapImages(string requestSearchTerm, int maxNumberOfImageUrls)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var driver = await _initializer.Initialize();
            var searchTerm = FormatTermToUrl(requestSearchTerm);
            var result = new List<SearchItem>();

            using var client = _clientProvider.CreateClient(HttpClientConstants.PornhubClient);
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                var pageNumber = i + 1;
                var fullpath = GetImagePageUrl(searchTerm, pageNumber);
                using var rtclient = _clientProvider.CreateClient(HttpClientConstants.RTPornhubClient);
                var html = await rtclient.GetStringAsync(fullpath);
                htmlDocument.LoadHtml(html);

                var photosAlbumSection = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//*[@id='photosAlbumsSection']");
                var albumNodes = photosAlbumSection
                    ?.SelectNodes("//li[contains(@class,'photoAlbumListContainer')]/div/a");

                if (albumNodes is null) continue;
                if (result.Count >= maxNumberOfImageUrls) break;

                foreach (var albumNode in albumNodes)
                {
                    var linkUrl = albumNode.Attributes["href"]?.Value;

                    if (linkUrl is null) continue;
                    if (result.Count >= maxNumberOfImageUrls) break;

                    var albumUrl = $"{client.BaseAddress}{linkUrl}";
                    driver.Navigate().GoToUrl(albumUrl);
                    var albumHtml = await client.GetStringAsync(driver.PageSource);
                    htmlDocument.LoadHtml(albumHtml);

                    var images = htmlDocument.DocumentNode
                        ?.SelectSingleNode("//ul[contains(@class, 'photosAlbumsListing')]")
                        ?.SelectNodes("//li/div");

                    if (images is not null)
                    {
                        foreach (var image in images)
                        {
                            var preview = image.Attributes["data-bkg"].Value;
                            var url = image.SelectSingleNode("/a").Attributes["href"].Value;
                            result.Add(new SearchItem
                            {
                                ImagePreviewUrl = preview,
                                SearchItemUrl = url,
                                Option = SearchOption.Image
                            });
                        }
                    }
                }
            }

            return result;
        }

        private async Task<List<SearchItem>> ScrapGifs(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> gifItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientConstants.PornhubClient);
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                var currentPageNumber = i + 1;
                // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"/gifs/search?search={searchTermUrlFormatted}&page={currentPageNumber}";
                var htmlSearchPage = await client.GetStringAsync(searchPageUrl);
                htmlDocument.LoadHtml(htmlSearchPage);

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var gifLinksNodes = bodyNode
                    ?.SelectNodes("//li[contains(@class, 'gifVideoBlock')]");

                if (gifLinksNodes is null || currentPageNumber == PAGE_NUMBER_LIMIT)
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

                    if (currentLinkGifNode is not null)
                    {
                        var currentLinkImageAttributes = currentLinkGifNode.Attributes;
                        searchVideoItem.ImagePreviewUrl = $"{client.BaseAddress}{currentLinkImageAttributes["href"].Value}";
                        searchVideoItem.SearchItemUrl =
                            $"{_dateSourcePhncdn}{currentLinkImageAttributes["href"].Value}.gif";
                    }

                    urlsCount++;
                    gifItems.Add(searchVideoItem);
                }

                await Task.Delay(1000);
            }

            return gifItems;
        }

        private static string GetImagePageUrl(string searchTerm, int pageNumber)
        {
            searchTerm = FormatTermToUrl(searchTerm);
            return "/albums?search={searchTerm}&page={pageNumber}";
        }
    }
}