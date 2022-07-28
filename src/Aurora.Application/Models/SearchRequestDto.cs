using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchRequestDto
    {
        public string SearchTerm { get; set; } = "";
        public List<SearchOption> SearchOptions { get; set; } = new List<SearchOption>();
        public List<SupportedWebsite> Websites { get; set; } = new List<SupportedWebsite>();
    }
}