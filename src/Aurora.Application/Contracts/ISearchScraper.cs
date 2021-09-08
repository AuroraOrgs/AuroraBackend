using Aurora.Application.Models;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Shared.Models;

namespace Aurora.Application.Contracts
{
    public interface ISearchScraper
    {
        Task<ValueOrNull<SearchResult>> Search(SearchRequest request, CancellationToken token = default);
    }
}
