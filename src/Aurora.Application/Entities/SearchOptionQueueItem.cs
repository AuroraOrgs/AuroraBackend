using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Entities;

public class SearchOptionQueueItem
{
    [Key]
    public Guid QueueId { get; set; }
    public Guid SearchOptionId { get; set; }

    public DateTime QueuedTimeUtc { get; set; }
    public bool IsProcessed { get; set; }

    public SearchRequestOption SearchOption { get; set; } = null!;
}
