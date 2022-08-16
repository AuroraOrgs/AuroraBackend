namespace Aurora.Application.Scrapers;

public interface ITotalScraperRunner
{
    Task RunTotalScraper(Type scraperType);
}