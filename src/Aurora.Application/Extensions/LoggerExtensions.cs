using Aurora.Application.Models;
using Microsoft.Extensions.Logging;
using Aurora.Shared.Extensions;
using Aurora.Application.Entities;

namespace Aurora.Application.Extensions
{
    public static class LoggerExtensions
    {
        //TODO: Get parameters for prefix through arguments
        public static void LogRequest(this ILogger logger, SearchRequestDto request, string prefix)
        {
            logger.LogInformation(
            "{prefix} for request for '{term}' term with '{options}' options in '{websites}' websites",
            prefix,
            request.SearchTerm,
            request.SearchOptions.CommaSeparate(),
            request.Websites.CommaSeparate()
           );
        }

        public static void LogRequest(this ILogger logger, SearchRequest request, string prefix)
        {
            logger.LogInformation(
            "{prefix} with request for '{term}' term with '{option}' option in '{website}' website",
            prefix,
            request.SearchTerm,
            request.ContentOption,
            request.Website
           );
        }
    }
}
