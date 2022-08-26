namespace Aurora.Application.Entities;

public class SearchRequestSnapshot
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }

    public Guid SearchRequestId { get; set; }
    public SearchRequest SearchRequest { get; set; } = null!;
}
