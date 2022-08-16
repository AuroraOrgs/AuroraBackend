namespace Aurora.Application.Models;

//TODO: Refactor double meaning of the term 'Request' (means set of content type and website for API request and one combination of option and website as entity)
public record SearchRequestState(Dictionary<(SupportedWebsite Website, ContentType ContentType, string Term), (Guid RequestId, SearchRequestStatus RequestStatus)> StoredRequests);
