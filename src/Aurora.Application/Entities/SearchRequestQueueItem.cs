using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Entities;

public class SearchRequestQueueItem
{
    [Key]
    public Guid QueueId { get; set; }
    public Guid SearchRequestId { get; set; }

    public DateTime QueuedTimeUtc { get; set; }
    public bool IsProcessed { get; set; }
}
