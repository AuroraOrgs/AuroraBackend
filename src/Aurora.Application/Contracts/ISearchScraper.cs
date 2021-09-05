using Aurora.Application.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface ISearchScraper
    {
        Task<SearchResult> Search(SearchRequest request, CancellationToken token = default);
    }
}
