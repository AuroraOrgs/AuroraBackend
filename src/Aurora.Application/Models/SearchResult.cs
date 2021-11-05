using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResult
    {
        public List<SearchItem> Items { get; }
        public int CountItems { get; set; }
        public string Website { get; set; }

        public SearchResult()
        {
            Items = new List<SearchItem>();
        }

        public SearchResult(List<SearchItem> searchItems)
        {
            Items = searchItems;
        }
    }
}