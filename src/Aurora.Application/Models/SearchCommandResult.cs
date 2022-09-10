using Aurora.Shared.Models;

namespace Aurora.Application.Models;

public record SearchCommandResult(List<OneOf<SearchResultDto, QueuedResult>> Results, long ItemsCount);
//TODO: Add date of queue
public record QueuedResult(SupportedWebsite Website, DateTime QueuedTime);