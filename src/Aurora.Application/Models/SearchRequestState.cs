using Aurora.Application.ValueObjects;

namespace Aurora.Application.Models;


//TODO: Refactor double meaning of the term 'Request' (means set of content type and website for API request and one combination of option and website as entity)
public record SearchRequestState
{
    public SearchRequestState(Dictionary<SearchRequestOptionDto, SearchRequestItem> storedRequests)
    {
        StoredOptions = storedRequests;
    }

    public Dictionary<SearchRequestOptionDto, SearchRequestItem> StoredOptions { get; set; }
};

public record SearchRequestOptionDto(SupportedWebsite Website, ContentType ContentType, SearchRequestTerm Term);

public record SearchRequestItem(Guid RequestId, SearchRequestStatus RequestStatus, List<SearchSnapshot> Snapshots);

public record SearchSnapshot(Guid SnapshotId, DateTime SnapshotTime);