using Aurora.Shared.Models;

namespace Aurora.Application.Models;

public record SearchCommandResult(List<OneOf<SearchResultDto, QueuedResult>> Results, long ItemsCount);
public record QueuedResult(SupportedWebsite Website, DateTime QueuedTime);