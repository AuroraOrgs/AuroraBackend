using System;
using System.Collections.Generic;
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

        public IEnumerable<SearchRequestToResult> Requests { get; set; } = null!;
    }
}
