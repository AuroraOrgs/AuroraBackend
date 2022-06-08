using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public record SearchResults(List<SearchResultDto> Results, long TotalItems, List<SupportedWebsite> ProcessedWebsites);
}
