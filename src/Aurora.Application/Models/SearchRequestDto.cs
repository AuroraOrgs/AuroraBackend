using Aurora.Domain.Enums;

namespace Aurora.Application.Models;

public class SearchRequestDto
{
    public List<string> SearchTerms { get; set; } = new List<string>();
    public List<ContentType> ContentTypes { get; set; } = new List<ContentType>();
    public List<SupportedWebsite> Websites { get; set; } = new List<SupportedWebsite>();
}