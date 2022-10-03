namespace Aurora.Application.Scrapers;

public interface IRefreshRunner
{
    Task RefreshAsync(CancellationToken token = default);
}
