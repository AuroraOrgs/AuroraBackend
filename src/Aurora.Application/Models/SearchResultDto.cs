using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResultDto
    {
        public List<SearchItem> Items { get; } = new List<SearchItem>();
        public string Website { get; set; }

        public SearchResultDto(List<SearchItem> searchItems, string website)
        {
            Items = searchItems;
            Website = website;
        }
    }
}