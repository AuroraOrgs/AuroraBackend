﻿using Aurora.Application.Models;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.Logging;

namespace Aurora.Application.Extensions;

public static class LoggerExtensions
{
    //TODO: Get parameters for prefix through arguments
    public static void LogRequest(this ILogger logger, SearchRequestDto request, string prefix)
    {
        logger.LogInformation(
        "{prefix} for request for '{term}' terms with '{types}' types in '{websites}' websites",
        prefix,
        request.SearchTerms.CommaSeparate(),
        request.ContentTypes.CommaSeparate(),
        request.Websites.CommaSeparate()
       );
    }
}
