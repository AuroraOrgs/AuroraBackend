using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface ISearchService
    {
        Task<List<SearchResultDto>> GetResults(SearchRequestDto request);
        Task StoreRequest(SearchRequestDto request);
        Task AddOrUpdateResults(SearchRequestDto request, IEnumerable<SearchResultDto> results);
    }
}
