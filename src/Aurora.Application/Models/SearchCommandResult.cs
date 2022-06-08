using System.Collections.Generic;

namespace Aurora.Application.Models
{
    public record SearchCommandResult(List<SearchResultDto> Results, long ItemsCount);
}
