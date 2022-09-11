using Aurora.Shared.Models;
using System.Collections.Immutable;

namespace Aurora.Application.Models;

public record GetSnapshostResult(ImmutableList<SearchResultDto> Results);
