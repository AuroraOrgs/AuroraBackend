using Aurora.Application.Commands;
using Aurora.Application.Contracts;
using Aurora.Application.Models;

namespace Aurora.Application.Scrapers;

public class RefreshRunner : IRefreshRunner
{
    private readonly ISearchStatisticsQueryService _stats;
    private readonly ISearchRepository _repo;
    private readonly IMediator _commandSender;

    public RefreshRunner(ISearchStatisticsQueryService stats, ISearchRepository repo, IMediator commandSender)
    {
        _stats = stats;
        _repo = repo;
        _commandSender = commandSender;
    }

    public async Task RefreshAsync(CancellationToken token = default)
    {
        var options = await _stats.QueryPopularOptionsAsync();
        if (options.Any())
        {
            var requests = options
                .GroupBy(x => x.Term)
                .Select(termOptions => new SearchRequestDto(termOptions.Key.Terms.ToList(), termOptions.Select(x => x.ContentType).ToList(), termOptions.Select(x => x.Website).ToList()));
            await Parallel.ForEachAsync(requests, async (request, token) =>
            {
                var state = await _repo.FetchRequest(request, isUserGenerated: false);
                await _repo.MarkAsQueued(state);
                var command = new ScrapCommand(request, userId: null);
                await _commandSender.Send(command, token);
            });
        }
    }
}
