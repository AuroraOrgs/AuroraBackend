using System;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Entities
{
    public class SearchResult
    {
        [Key]
        public Guid Id { get; set; }
        public string? ImagePreviewUrl { get; set; }
        public string? SearchItemUrl { get; set; }
        public DateTime FoundTimeUtc { get; set; }

        public Guid RequestId { get; set; }
        public SearchRequest Request { get; set; }
    }
}
