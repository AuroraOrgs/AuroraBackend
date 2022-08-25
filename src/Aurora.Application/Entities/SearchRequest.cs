using Aurora.Application.Models;
using Aurora.Application.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Entities;

public class SearchRequest
{
    [Key]
    public Guid Id { get; set; }

    public SearchRequestTerm SearchTerm { get; set; } = null!;
    public SupportedWebsite Website { get; set; }
    public ContentType ContentType { get; set; }

    public int OccurredCount { get; set; }
    public IEnumerable<SearchRequestQueueItem> QueueItems { get; set; } = null!;
}
