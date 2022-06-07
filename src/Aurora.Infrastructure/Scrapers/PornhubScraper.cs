using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Models;
using Aurora.Infrastructure.Contracts;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;
using Aurora.Infrastructure.Services;
using Aurora.Application.Enums;
using HtmlAgilityPack;
using System.Net;

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubScraper : ScraperBase
    {
        private const int PAGE_NUMBER_LIMIT = 5;
        private const string _baseUrl = "https://www.pornhub.com";
        private const string _dateSourcePhncdn = "https://dl.phncdn.com";
        private readonly IWebClientService _clientProvider;
        private readonly DriverInitializer _initializer;

        public override SupportedWebsite WebSite => SupportedWebsite.Pornhub;

        public PornhubScraper(ILogger<PornhubScraper> logger, IWebClientService clientProvider,
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

            var urlsCount = 0;

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/video/search?search={searchTermUrlFormatted}&page={i + 1}";
                await _clientProvider.SetDefaultUserString(client);
                bool isEnd = LoadDocumentFromUrl(htmlDocument, client, searchPageUrl);

                if (isEnd)
                {
                    break;
                }

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode
                    ?.SelectNodes("//a[contains(@class, 'videoPreviewBg')]");

                if (videoLinksNodes is not null)
                {
                    foreach (var videoLinkNode in videoLinksNodes)
                    {
                        var currentLinkImageNode = videoLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "img");
                        //TODO: Add default image or make preview image nullable
                        string previewImage = null;

                        if (currentLinkImageNode is not null)
                        {
                            var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                            previewImage = currentLinkImageAttributes["data-thumb_url"].Value;
                        }

                        var currentLinkAttributes = videoLinkNode.Attributes;
                        string itemUrl = $"{_baseUrl}{currentLinkAttributes["href"].Value}";

                        urlsCount++;
                        videoItems.Add(new(SearchOption.Video, previewImage, itemUrl));
                    }
                }

                await Task.Delay(1000);
            }

            return videoItems;
        }

        private static bool LoadDocumentFromUrl(HtmlDocument htmlDocument, WebClient client, string searchPageUrl)
        {
            bool reachedEnd;
            try
            {
                var htmlSearchPage = client.DownloadString(searchPageUrl);
                htmlDocument.LoadHtml(htmlSearchPage);
                reachedEnd = false;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                {
                    reachedEnd = true;
                }
                else
                {
                    throw;
                }
            }
            return reachedEnd;
        }

        private async Task<List<SearchItem>> ScrapImages(string requestSearchTerm, int maxNumberOfImageUrls)
        {
            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var driver = await _initializer.Initialize();
            var searchTerm = FormatTermToUrl(requestSearchTerm);
            var result = new List<SearchItem>();

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                var pageNumber = i + 1;
                var fullUrl = GetImagePageUrl(searchTerm, pageNumber);
                await _clientProvider.SetDefaultUserString(client);
                var html = client.DownloadString(fullUrl);
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

                    await _clientProvider.SetDefaultUserString(client);
                    var albumUrl = $"{_baseUrl}{linkUrl}";
                    driver.Navigate().GoToUrl(albumUrl);
                    var albumHtml = client.DownloadString(driver.PageSource);
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
                            result.Add(new SearchItem(SearchOption.Image, preview, url));
                        }
                    }
                }
            }

            return result;
        }

        private async Task<List<SearchItem>> ScrapGifs(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> gifItems = new();

            var htmlDocument = new HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var urlsCount = 0;

            using var client = await _clientProvider.Provide();

            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                var currentPageNumber = i + 1;
                // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={currentPageNumber}";
                await _clientProvider.SetDefaultUserString(client);
                bool isEnd = LoadDocumentFromUrl(htmlDocument, client, searchPageUrl);
                if (isEnd)
                {
                    break;
                }
                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var gifLinksNodes = bodyNode
                    ?.SelectNodes("//li[contains(@class, 'gifVideoBlock')]");

                if (gifLinksNodes is not null)
                {
                    foreach (var gifLinkNode in gifLinksNodes)
                    {
                        var currentLinkGifNode = gifLinkNode.ChildNodes
                            .FirstOrDefault(n => n.Name == "a");

                        if (currentLinkGifNode is not null)
                        {
                            var currentLinkImageAttributes = currentLinkGifNode.Attributes;
                            string imagePreviewUrl = $"{_baseUrl}{currentLinkImageAttributes["href"].Value}";
                            string searchItemUrl = $"{_dateSourcePhncdn}{currentLinkImageAttributes["href"].Value}.gif";
                            gifItems.Add(new(SearchOption.Gif, imagePreviewUrl, searchItemUrl));
                        }

                        urlsCount++;
                    }
                }

                await Task.Delay(1000);
            }

            return gifItems;
        }

        private static string GetImagePageUrl(string searchTerm, int pageNumber)
        {
            searchTerm = FormatTermToUrl(searchTerm);
            return $"https://rt.pornhub.com/albums?search={searchTerm}&page={pageNumber}";
        }
    }
}