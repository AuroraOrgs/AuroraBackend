namespace Aurora.Scrapers.Discovery;

public class ScrapersContext
{
    public ScrapersContext(List<Type> totalScrapers, List<Type> optionScrapers)
    {
        TotalScrapers = totalScrapers;
        OptionScrapers = optionScrapers;
    }

    public List<Type> TotalScrapers { get; }
    public List<Type> OptionScrapers { get; }
}
