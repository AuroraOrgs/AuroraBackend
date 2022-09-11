namespace Aurora.Application.Models;

public record GetSnapshotsResult(Dictionary<SearchRequestOptionDto, List<SearchSnapshot>> OptionToSnapshots);
