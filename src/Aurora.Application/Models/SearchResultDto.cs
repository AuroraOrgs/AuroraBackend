namespace Aurora.Application.Models;

public class SearchResultDto
{
    public List<SearchItem> Items { get; }
    public SupportedWebsite Website { get; set; }
    public List<string> Terms { get; set; }

    public SearchResultDto(List<SearchItem> searchItems, List<string> terms, SupportedWebsite website)
    {
        Items = searchItems;
        Website = website;
        Terms = terms;
    }
}