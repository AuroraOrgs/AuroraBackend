using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchResult
    {
        public List<SearchItem> Items { get; set; }
        public string Website { get; set; }
    }
}