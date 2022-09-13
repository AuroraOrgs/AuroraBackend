using Aurora.Application.Models;

namespace Aurora.Application.Contracts;

public interface ISearchStatistics
{
    Task<IEnumerable<SearchRequestOptionDto>> QueryPopularRequests();
}
