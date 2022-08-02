using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface IOptionScraper
    {
        SupportedWebsite Website { get; }
        IEnumerable<SearchOption> Options { get; }
        Task<List<SearchItem>> ScrapAsync(string term, CancellationToken token = default);
    }
}
