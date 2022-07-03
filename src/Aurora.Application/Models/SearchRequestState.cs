using Aurora.Application.Models;
using System;
using System.Collections.Generic;

namespace Aurora.Application.Models
{
    //TODO: Refactor double meaning of the term 'Request' (means set of options and website for API request and one combination of option and website as entity)
    public record SearchRequestState(Dictionary<(SupportedWebsite, SearchOption), (Guid RequestId, SearchRequestStatus RequestStatus)> StoredRequests);
}
