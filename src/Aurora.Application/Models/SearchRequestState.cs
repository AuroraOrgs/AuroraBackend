using Aurora.Application.ValueObjects;

namespace Aurora.Application.Models;


//TODO: Refactor double meaning of the term 'Request' (means set of content type and website for API request and one combination of option and website as entity)
public record SearchRequestState
{
    public SearchRequestState(Dictionary<SearchRequestOption, SearchRequestItem> storedRequests)
    {
        StoredRequests = storedRequests;
    }

    public Dictionary<SearchRequestOption, SearchRequestItem> StoredRequests { get; set; }
};

public record SearchRequestOption(SupportedWebsite Website, ContentType ContentType, SearchRequestTerm Term);

public record SearchRequestItem(Guid RequestId, SearchRequestStatus RequestStatus, List<SearchSnapshot> Snapshots);

public record SearchSnapshot(Guid SnapshotId, DateTime SnapshotTime);