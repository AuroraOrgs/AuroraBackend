using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchRequest
    {
        public string SearchTerm { get; set; }
        public List<SearchOption> SearchOptions { get; set; }
        public int ResponseItemsMaxCount { get; set; }
        public List<string> Websites { get; set; }
    }
}