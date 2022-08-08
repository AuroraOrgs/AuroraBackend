using Aurora.Application.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Entities
{
    public class SearchRequest
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string SearchTerm { get; set; } = null!;
        public SupportedWebsite Website { get; set; }
        public ContentType ContentType { get; set; }

        public int OccurredCount { get; set; }
        public IEnumerable<SearchRequestQueueItem> QueueItems { get; set; } = null!;
    }
}
