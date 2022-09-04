using Aurora.Domain.Enums;
using Aurora.Domain.ValueObjects;
using Aurora.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Domain.Entities;

public class SearchRequestOption
{
    public Guid Id { get; set; }

    public SearchOptionTerm SearchTerm { get; set; } = null!;
    public EnumWrapper<SupportedWebsite> Website { get; set; } = null!;
    public EnumWrapper<ContentType> ContentType { get; set; } = null!;

    public int OccurredCount { get; set; }
    public IEnumerable<SearchOptionSnapshot> Snapshots { get; set; } = null!;
}
