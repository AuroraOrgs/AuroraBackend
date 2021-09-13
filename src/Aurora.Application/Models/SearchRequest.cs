using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchRequest
    {
        public string SearchTerm { get; set; }
        public List<SearchOption> SearchOptions { get; set; }
        public int ResponseWebsitesMaxCount { get; set; }
        public List<string> Websites { get; set; }
    }
}