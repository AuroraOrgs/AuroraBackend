using Aurora.Application.Enums;
using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResultDto
    {
        public List<SearchItem> Items { get; } = new List<SearchItem>();
        public SupportedWebsite Website { get; set; }
        public bool BeenQueued { get; set; }

        public SearchResultDto(SupportedWebsite website)
        {
            Items = null;
            BeenQueued = true;
            Website = website;
        }

        public SearchResultDto(List<SearchItem> searchItems, SupportedWebsite website)
        {
            Items = searchItems;
            BeenQueued = false;
            Website = website;
        }
    }
}