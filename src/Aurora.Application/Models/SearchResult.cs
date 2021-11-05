using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResult
    {
        public List<SearchItem> Items { get; } = new List<SearchItem>();
        public int CountItems { get; set; }
        public string Website { get; set; }

        public SearchResult(List<SearchItem> searchItems, string website)
        {
            Items = searchItems;
            Website = website;
        }
    }
}