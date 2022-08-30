using Aurora.Domain.Enums;
using Aurora.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Domain.Entities;

public class SearchRequestOption
{
    [Key]
    public Guid Id { get; set; }

    public SearchOptionTerm SearchTerm { get; set; } = null!;
    public SupportedWebsite Website { get; set; }
    public ContentType ContentType { get; set; }

    public int OccurredCount { get; set; }
    public IEnumerable<SearchOptionSnapshot> Snapshots { get; set; } = null!;
}
