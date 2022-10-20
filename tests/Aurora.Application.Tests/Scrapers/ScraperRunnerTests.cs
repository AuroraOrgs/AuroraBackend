using Aurora.Application.Config;
using Aurora.Application.Models;
using Aurora.Application.Scrapers;
using Aurora.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Aurora.Application.Tests.Scrapers;

public class ScraperRunnerTests
{
    [Fact]
    public async Task RunAsync_ShouldExecuteCallbackForResults_WhenCallbackProvided()
    {
        //Arrange
        var website = SupportedWebsite.XVideos;
        ScraperRunner sut = SetupRunnerFor(website);

        //Act 
        var request = new SearchRequestDto(new List<string> { "test" }, ContentTypeContext.ContentTypes, new List<SupportedWebsite>() { website });
        var resultsCount = 0;
        await sut.RunAsync(request, result =>
        {
            resultsCount++;
            return Task.CompletedTask;
        });
        //Assert
        resultsCount.Should().Be(1);
    }

    private static ScraperRunner SetupRunnerFor(SupportedWebsite website)
    {
        var items = new List<SearchItem>()
        {
            new SearchItem(ContentType.Image, "preview", "itemUrl")
        };
        var collector = new Mock<IOptionsScraperCollector>();
        var scraper = new Mock<IOptionScraper>();
        scraper.Setup(x => x.ScrapAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(items);
        scraper.Setup(x => x.Website).Returns(website);
        IEnumerable<IOptionScraper> scrapers = new List<IOptionScraper>()
        {
            scraper.Object
        };
        collector.Setup(x => x.CollectFor(It.IsAny<IEnumerable<(SupportedWebsite Key, ContentType value)>>())).Returns(ValueTask.FromResult(scrapers));
        return new ScraperRunner(collector.Object, NullLogger<ScraperRunner>.Instance, Options.Create(new RunnerConfig { MaxConcurrentScrapers = 5 }));
    }
}
