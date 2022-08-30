using Aurora.Domain.ValueObjects;

namespace Aurora.Application.Models;

public record SearchRequestState
{
    public SearchRequestState(Dictionary<SearchRequestOptionDto, SearchRequestOptionItem> storedRequests)
    {
        StoredOptions = storedRequests;
    }

    public Dictionary<SearchRequestOptionDto, SearchRequestOptionItem> StoredOptions { get; set; }
};

public record SearchRequestOptionDto(SupportedWebsite Website, ContentType ContentType, SearchOptionTerm Term);

public record SearchRequestOptionItem(Guid OptionId, SearchRequestOptionStatus OptionStatus, List<SearchSnapshot> Snapshots);

public record SearchSnapshot(Guid SnapshotId, DateTime SnapshotTime);