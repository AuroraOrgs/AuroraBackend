using Aurora.Application.Models;
using System.Threading.Tasks;

namespace Aurora.Application.Contracts
{
    public interface ISearchScraper
    {
        Task<SearchResult> Search(SearchRequest request);
    }
}
