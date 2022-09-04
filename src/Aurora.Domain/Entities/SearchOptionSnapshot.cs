namespace Aurora.Domain.Entities;

public class SearchOptionSnapshot
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public bool IsProcessed { get; set; }

    public Guid SearchOptionId { get; set; }
    public SearchRequestOption SearchOption { get; set; } = null!;
}
