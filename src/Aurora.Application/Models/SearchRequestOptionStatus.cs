namespace Aurora.Application.Models;

public enum SearchRequestOptionStatus
{
    /// <summary>
    /// Search request is being processed
    /// </summary>
    Queued,
    /// <summary>
    /// Search request is not being processed and has never been
    /// </summary>
    NotFetched,
    /// <summary>
    /// Search request is processed
    /// </summary>
    Fetched
}
