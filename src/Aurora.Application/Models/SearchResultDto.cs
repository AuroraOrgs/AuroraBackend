using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResultDto
    {
        //TODO: Add generic type preservation to SearchResultDto
        public List<SearchItem<SearchResultData>>? Items { get; }
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

        public SearchResultDto(List<SearchItem<SearchResultData>> searchItems, List<string> terms, SupportedWebsite website)
        {
            Items = searchItems;
            BeenQueued = false;
            Website = website;
            Terms = terms;
        }
    }
}