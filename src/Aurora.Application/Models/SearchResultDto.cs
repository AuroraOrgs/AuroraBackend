using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResultDto
    {
        public List<SearchItem>? Items { get; }
        public SupportedWebsite Website { get; set; }
        public List<string> Terms { get; set; }
        public bool BeenQueued { get; set; }

        public SearchResultDto(List<string> terms, SupportedWebsite website)
        {
            Items = null;
            BeenQueued = true;
            Website = website;
            Terms = terms;
        }

        public SearchResultDto(List<SearchItem> searchItems, List<string> terms, SupportedWebsite website)
        {
            Items = searchItems;
            BeenQueued = false;
            Website = website;
            Terms = terms;
        }
    }
}