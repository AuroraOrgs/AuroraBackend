using System.Collections.Immutable;

namespace Aurora.Application.Models;

public record FindSnapshotsResult(ImmutableDictionary<SearchRequestOptionDto, List<SearchSnapshot>> OptionToSnapshots);
