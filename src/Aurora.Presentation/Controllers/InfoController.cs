using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Aurora.Presentation.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IOptionsScraperCollector _scraperCollector;

        public InfoController(IOptionsScraperCollector scraperCollector)
        {
            _scraperCollector = scraperCollector;
        }

        [HttpGet("supported-websites")]
        public IActionResult SupportedWebsites()
        {
            return Ok(EnumHelper.EnumValueToName<SupportedWebsite>());
        }

        [HttpGet("supported-content-types")]
        public IActionResult SupportedContentTypes()
        {
            return Ok(EnumHelper.EnumValueToName<SearchOption>());
        }

        [HttpGet("supported-search-options")]
        public IActionResult SupportedSearchOptions()
        {
            var allowedOptions = _scraperCollector.AllowedKeys.GroupBy(x => x.Key).ToDictionary(x => (int)x.Key, x => x.Select(x => x.value).ToList());
            return Ok(allowedOptions);
        }
    }
}
