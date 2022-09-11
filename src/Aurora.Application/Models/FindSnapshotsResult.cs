namespace Aurora.Application.Models;

public record FindSnapshotsResult(Dictionary<SearchRequestOptionDto, List<SearchSnapshot>> OptionToSnapshots);
