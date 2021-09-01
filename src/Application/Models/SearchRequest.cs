using System.Collections.Generic;
using Aurora.Application.Enums;

namespace Aurora.Application.Models
{
    public class SearchRequest
    {
        public string SearchTerm { get; set; }
        public List<SearchOption> SearchOptions { get; set; }
    }
}