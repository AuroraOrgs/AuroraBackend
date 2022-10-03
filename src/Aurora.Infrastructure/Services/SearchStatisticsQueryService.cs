using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Infrastructure.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aurora.Infrastructure.Services;

internal class SearchStatisticsQueryService : ISearchStatisticsQueryService
{
    private readonly SearchContext _ctx;
    private readonly IOptions<RefreshConfig> _options;

    public SearchStatisticsQueryService(SearchContext ctx, IOptions<RefreshConfig> options)
    {
        _ctx = ctx;
        _options = options;
    }

    public async Task<IEnumerable<SearchRequestOptionDto>> QueryPopularOptionsAsync()
    {
        var config = _options.Value;
        return await _ctx.Options.OrderByDescending(x => x.OccurredCount)
                                                 .Take(config.RefreshOptionsCount)
                                                 .Select(option => new SearchRequestOptionDto(option.Website, option.ContentType, option.SearchTerm))
                                                 .ToListAsync();
    }
}
