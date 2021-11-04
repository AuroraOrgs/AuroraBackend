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

namespace Aurora.Infrastructure.Scrapers
{
    public class PornhubScraper : ScraperBase
    {
        private const int PAGE_NUMBER_LIMIT = 5;
        private const string _baseUrl = "https://www.pornhub.com";
        private const string _dateSourcePhncdn = "https://dl.phncdn.com";
        private readonly IWebClientService _clientProvider;
        private readonly DriverInitializer _initializer;

        public PornhubScraper(ILogger<PornhubScraper> logger, IWebClientService clientProvider,
            DriverInitializer initializer) : base(logger)
        {
            _clientProvider = clientProvider;
            _initializer = initializer;
        }

        public override async Task<ValueOrNull<SearchResult>> SearchVideosInner(
            SearchRequest request,
            CancellationToken token = default)
        {
            SearchResult result = new()
            {
                Website = _baseUrl
            };

            try
            {
                var videos = await ScrapVideos(request.SearchTerm, request.ResponseItemsMaxCount);
                result.Items.AddRange(videos);
            }
            catch (Exception exception)
            {
                return ValueOrNull<SearchResult>.CreateNull(exception.Message);
            }

            result.CountItems = result.Items.Count;
            return result;
        }

        public override async Task<ValueOrNull<SearchResult>> SearchGifsInner(
            SearchRequest request,
            CancellationToken token = default)
        {
            SearchResult result = new()
            {
                Website = _baseUrl
            };

            try
            {
                var videos = await ScrapGifs(request.SearchTerm, request.ResponseItemsMaxCount);
                result.Items.AddRange(videos);
            }
            catch (Exception exception)
            {
                return ValueOrNull<SearchResult>.CreateNull(exception.Message);
            }

            result.CountItems = result.Items.Count;
            return result;
        }

        public override async Task<ValueOrNull<SearchResult>> SearchImagesInner(
            SearchRequest request,
            CancellationToken token = default)
        {
            SearchResult result = new()
            {
                Website = _baseUrl
            };

            try
            {
                var videos = await ScrapImages(request.SearchTerm, request.ResponseItemsMaxCount);
                result.Items.AddRange(videos);
            }
            catch (Exception exception)
            {
                return ValueOrNull<SearchResult>.CreateNull(exception.Message);
            }

            result.CountItems = result.Items.Count;
            return result;
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

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                // e.g: https://www.pornhub.com/video/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/video/search?search={searchTermUrlFormatted}&page={pageNumber}";
                await _clientProvider.SetDefaultUserString(client);
                var htmlSearchPage = client.DownloadString(searchPageUrl);
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
                    searchVideoItem.SearchItemUrl = $"{_baseUrl}{currentLinkAttributes["href"].Value}";

                    urlsCount++;
                    videoItems.Add(searchVideoItem);
                }

                pageNumber++;
                await Task.Delay(1000);
            }

            return videoItems;
        }

        private async Task<IEnumerable<SearchItem>> ScrapImages(string requestSearchTerm, int maxNumberOfImageUrls)
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument
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
                            result.Add(new SearchItem
                            {
                                ImagePreviewUrl = preview, SearchItemUrl = url, Option = SearchOption.Image
                            });
                        }
                    }
                }
            }

            return result;
        }

        private async Task<IEnumerable<SearchItem>> ScrapGifs(string searchTerm, int maxNumberOfVideoUrls)
        {
            List<SearchItem> gifItems = new();

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
                
                var currentPageNumber = i + 1;
                // e.g: https://www.pornhub.com/gifs/search?search=test+value&page=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/gifs/search?search={searchTermUrlFormatted}&page={currentPageNumber}";
                await _clientProvider.SetDefaultUserString(client);
                var htmlSearchPage = client.DownloadString(searchPageUrl);
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
                        searchVideoItem.ImagePreviewUrl = $"{_baseUrl}{currentLinkImageAttributes["href"].Value}";
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
            return $"https://rt.pornhub.com/albums?search={searchTerm}&page={pageNumber}";
        }
    }
}