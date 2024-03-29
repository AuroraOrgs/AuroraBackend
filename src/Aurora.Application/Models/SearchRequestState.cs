﻿using Aurora.Domain.ValueObjects;
using System.Collections.Immutable;

namespace Aurora.Application.Models;

public record SearchRequestState
{
    public SearchRequestState(ImmutableDictionary<SearchRequestOptionDto, SearchRequestOptionItem> storedRequests)
    {
        StoredOptions = storedRequests;
    }

    public ImmutableDictionary<SearchRequestOptionDto, SearchRequestOptionItem> StoredOptions { get; }
};

public record SearchRequestOptionDto(SupportedWebsite Website, ContentType ContentType, SearchOptionTerm Term);

public record SearchRequestOptionItem(Guid OptionId, List<SearchSnapshot> Snapshots);

public record SearchSnapshot(Guid SnapshotId, DateTime SnapshotTime, bool IsProcessed);