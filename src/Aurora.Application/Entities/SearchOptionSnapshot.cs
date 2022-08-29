namespace Aurora.Application.Entities;

public class SearchOptionSnapshot
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }

    public Guid SearchOptionId { get; set; }
    public SearchRequestOption SearchOption { get; set; } = null!;
}
