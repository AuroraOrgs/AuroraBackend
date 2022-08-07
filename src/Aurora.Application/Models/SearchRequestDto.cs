using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public class SearchRequestDto
    {
        public string SearchTerm { get; set; } = "";
        public List<ContentType> ContentTypes { get; set; } = new List<ContentType>();
        public List<SupportedWebsite> Websites { get; set; } = new List<SupportedWebsite>();
    }
}