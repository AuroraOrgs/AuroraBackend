using System.Collections.Immutable;

namespace Aurora.Application.Models;

public record FindSnapshotsResult(ImmutableList<SnapshotsResult> Results);

public record SnapshotsResult(SearchRequestOptionDto Option, List<SearchSnapshot> Snapshots);
