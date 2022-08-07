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
        public string SearchTerm { get; set; }
        public SupportedWebsite Website { get; set; }
        public ContentType ContentOption { get; set; }

        public int OccurredCount { get; set; }
        public IEnumerable<SearchRequestQueueItem> QueueItems { get; set; }
    }
}
