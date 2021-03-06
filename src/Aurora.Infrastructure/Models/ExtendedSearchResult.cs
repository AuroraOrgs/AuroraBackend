using Aurora.Application.Models;
using Aurora.Shared.Models;

namespace Aurora.Infrastructure.Models
{
    public class ExtendedSearchResult
    {
        public ValueOrNull<SearchResultDto> Result { get; set; }
        public ScraperStatusCode StatusCode { get; set; }
    }
}
