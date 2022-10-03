namespace Aurora.Application.Models;

public class SearchRequestDto
{
    public SearchRequestDto()
    {

    }

    public SearchRequestDto(List<string> searchTerms, List<ContentType> contentTypes, List<SupportedWebsite> websites)
    {
        SearchTerms = searchTerms;
        ContentTypes = contentTypes;
        Websites = websites;
    }

    public List<string> SearchTerms { get; set; } = new List<string>();
    public List<ContentType> ContentTypes { get; set; } = new List<ContentType>();
    public List<SupportedWebsite> Websites { get; set; } = new List<SupportedWebsite>();
}