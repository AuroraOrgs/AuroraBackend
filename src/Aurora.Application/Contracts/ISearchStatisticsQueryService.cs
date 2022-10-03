using Aurora.Application.Models;

namespace Aurora.Application.Contracts;

public interface ISearchStatisticsQueryService
{
    Task<IEnumerable<SearchRequestOptionDto>> QueryPopularOptionsAsync();
}
