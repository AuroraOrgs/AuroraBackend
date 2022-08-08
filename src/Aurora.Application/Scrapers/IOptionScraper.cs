using Aurora.Application.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Application.Scrapers
{
    public interface IOptionScraper
    {
        SupportedWebsite Website { get; }
        IEnumerable<ContentType> ContentTypes { get; }
        Task<List<SearchItem>> ScrapAsync(List<string> terms, CancellationToken token = default);
    }
}
