using Aurora.Application.Models;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Shared.Models;

namespace Aurora.Application.Contracts
{
    public interface ISearchScraper
    {
        Task<ValueOrNull<SearchResultDto>> Search(SearchRequestDto request, CancellationToken token = default);
    }
}
