namespace Aurora.Application.Models;

public enum SearchRequestOptionStatus
{
    Queued, //search request is being processed
    NotFetched, //search request is not being processed and has never been
    Fetched //search request is processed
}
