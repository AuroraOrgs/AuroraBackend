using Aurora.Application.Models;
using System;
using System.Collections.Generic;

namespace Aurora.Application.Models
{
    //TODO: Refactor double meaning of the term 'Request' (means set of content type and website for API request and one combination of option and website as entity)
    public record SearchRequestState(Dictionary<(SupportedWebsite, ContentType), (Guid RequestId, SearchRequestStatus RequestStatus)> StoredRequests);
}
