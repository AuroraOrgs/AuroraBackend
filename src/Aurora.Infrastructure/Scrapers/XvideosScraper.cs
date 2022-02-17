using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application;
using Aurora.Application.Enums;
using Aurora.Application.Models;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Scrapers
{
    public class XvideosScraper : ScraperBase
    {
        private const int PAGE_NUMBER_LIMIT = 5;
        private readonly IHttpClientFactory _clientProvider;

        public override SupportedWebsite WebSite => SupportedWebsite.Xvideos;

        public XvideosScraper(ILogger<XvideosScraper> logger, IHttpClientFactory clientProvider) : base(logger)
        {
            _clientProvider = clientProvider;
        }

        public override async Task<ValueOrNull<List<SearchItem>>> SearchVideosInner(
            SearchRequestDto request,
            CancellationToken token = default)
        {
            return await ScrapVideos(request.SearchTerm, request.ResponseItemsMaxCount);
        }

        public override async Task<ValueOrNull<List<SearchItem>>> SearchImagesInner(
            SearchRequestDto request,
            CancellationToken token = default)
        {
            return await ScrapImages(request.SearchTerm, request.ResponseItemsMaxCount);
        }

        private async Task<List<SearchItem>> ScrapVideos(string searchTerm,
            int maxNumberOfVideoUrls)
        {
            List<SearchItem> videoItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientConstants.XvideosClient);
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"/?k={searchTermUrlFormatted}&p={pageNumber}";
                var htmlSearchPage = await client.GetStringAsync(searchPageUrl);

                htmlDocument.LoadHtml(htmlSearchPage);

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode
                    ?.SelectNodes("//a");

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
                        searchVideoItem.ImagePreviewUrl = currentLinkImageAttributes["data-src"]?.Value;
                    }

                    var currentLinkAttributes = videoLinkNode.Attributes;
                    var videoLink = currentLinkAttributes["href"]?.Value;
                    if (videoLink is not null && videoLink.Contains("video") && !videoLink.Contains("videos"))
                        searchVideoItem.SearchItemUrl = $"{client.BaseAddress}{videoLink}";

                    urlsCount++;
                    if (searchVideoItem.SearchItemUrl is not null && searchVideoItem.ImagePreviewUrl is not null)
                        videoItems.Add(searchVideoItem);
                }

                pageNumber++;
                await Task.Delay(1000);
            }

            return videoItems;
        }

        private async Task<List<SearchItem>> ScrapImages(string searchTerm,
            int maxNumberOfImageUrls)
        {
            List<SearchItem> imageItems = new();

            var htmlDocument = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true
            };

            var pageNumber = 1;

            var urlsCount = 0;

            using var client = _clientProvider.CreateClient(HttpClientConstants.XvideosClient);
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfImageUrls)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"/?k={searchTermUrlFormatted}&p={pageNumber}";
                var htmlSearchPage = await client.GetStringAsync(searchPageUrl);

                htmlDocument.LoadHtml(htmlSearchPage);

                var bodyNode = htmlDocument.DocumentNode
                    ?.SelectSingleNode("//body");
                var videoLinksNodes = bodyNode
                    ?.SelectNodes("//a");

                if (videoLinksNodes is null)
                {
                    break;
                }

                foreach (var videoLinkNode in videoLinksNodes)
                {
                    SearchItem searchImageItem = new()
                    {
                        Option = SearchOption.Image
                    };

                    var currentLinkImageNode = videoLinkNode.ChildNodes
                        .FirstOrDefault(n => n.Name == "img");

                    if (currentLinkImageNode is not null)
                    {
                        var currentLinkImageAttributes = currentLinkImageNode.Attributes;
                        searchImageItem.SearchItemUrl = currentLinkImageAttributes["data-src"]?.Value;
                    }

                    urlsCount++;
                    if (searchImageItem.SearchItemUrl is not null)
                        imageItems.Add(searchImageItem);
                }

                pageNumber++;
                await Task.Delay(1000);
            }

            return imageItems;
        }
    }
}