using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Application.Models;
using Aurora.Infrastructure.Contracts;
using Aurora.Infrastructure.Services;
using Aurora.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Scrapers
{
    public class XvideosScraper : ScraperBase
    {
        private const int PAGE_NUMBER_LIMIT = 5;
        private const string _baseUrl = "https://www.xvideos.com";
        private readonly IWebClientService _clientProvider;

        public XvideosScraper(ILogger<XvideosScraper> logger, IWebClientService clientProvider) : base(logger)
        {
            _clientProvider = clientProvider;
        }

        public override async Task<SearchResult> SearchVideosInner(
            SearchRequest request,
            CancellationToken token = default)
        {
            var videos = await ScrapVideos(request.SearchTerm, request.ResponseItemsMaxCount);

            return new SearchResult(videos)
            {
                Website = _baseUrl
            };
        }

        public override async Task<SearchResult> SearchImagesInner(
            SearchRequest request,
            CancellationToken token = default)
        {
            var images = await ScrapImages(request.SearchTerm, request.ResponseItemsMaxCount);

            return new SearchResult(images)
            {
                Website = _baseUrl
            };
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

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfVideoUrls)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
                await _clientProvider.SetTls12UserString(client);
                var htmlSearchPage = client.DownloadString(searchPageUrl);
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
                        searchVideoItem.SearchItemUrl = $"{_baseUrl}{videoLink}";

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

            using var client = await _clientProvider.Provide();
            for (var i = 0; i < PAGE_NUMBER_LIMIT; i++)
            {
                if (urlsCount >= maxNumberOfImageUrls)
                {
                    break;
                }

                // e.g: https://www.xvideos.com/?k=test+value&p=1
                var searchTermUrlFormatted = FormatTermToUrl(searchTerm);
                var searchPageUrl = $"{_baseUrl}/?k={searchTermUrlFormatted}&p={pageNumber}";
                await _clientProvider.SetTls12UserString(client);
                var htmlSearchPage = client.DownloadString(searchPageUrl);
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