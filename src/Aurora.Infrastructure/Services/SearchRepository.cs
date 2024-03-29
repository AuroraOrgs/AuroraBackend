﻿using Aurora.Application.Contracts;
using Aurora.Application.Models;
using Aurora.Domain.Entities;
using Aurora.Domain.ValueObjects;
using Aurora.Shared.Extensions;
using Aurora.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Aurora.Infrastructure.Services;

public class SearchRepository : ISearchRepository
{
    private readonly SearchContext _context;
    private readonly ILogger<SearchRepository> _logger;
    private readonly IDateTimeProvider _dateTime;

    public SearchRepository(SearchContext context, ILogger<SearchRepository> logger, IDateTimeProvider dateTime)
    {
        _context = context;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<SearchRequestState> FetchRequest(SearchRequestDto request, bool isUserGenerated)
    {
        var term = SearchOptionTerm.CreateAnd(request.SearchTerms);
        var storedOptions = await _context.Options
                        .Include(x => x.Snapshots)
                        .Where(x =>
                            request.ContentTypes.Contains(x.ContentType)
                            && request.Websites.Contains(x.Website)
                            && x.SearchTerm == term)
                        .ToListAsync();

        var existingOptions = storedOptions.Select(x => new SearchRequestOptionDto(x.Website, x.ContentType, x.SearchTerm));
        List<SearchRequestOptionDto> requestedOptions = new();
        foreach (var website in request.Websites)
        {
            foreach (var option in request.ContentTypes)
            {
                requestedOptions.Add(new(website, option, term));
            }
        }
        var newOptionModels = requestedOptions.Where(x => existingOptions.NotContains(x));
        var newOptions = await CreateOptionsAsync(newOptionModels);

        if (isUserGenerated)
        {
            foreach (var existingRequest in storedOptions)
            {
                existingRequest.OccurredCount++;
            }
        }
        _context.Options.UpdateRange(storedOptions);

        var updatedCount = await _context.SaveChangesAsync();
        _logger.LogInformation("Updated '{number}' records whilst fetching", updatedCount);
        var allOptions = storedOptions.Union(newOptions).ToList();
        var result = allOptions.ToImmutableDictionary(
            key => new SearchRequestOptionDto(key.Website, key.ContentType, key.SearchTerm),
            value => new SearchRequestOptionItem(
                value.Id,
                value.Snapshots.Select(snapshot => new SearchSnapshot(snapshot.Id, snapshot.Time, snapshot.IsProcessed)).ToList()
              ));
        return new SearchRequestState(result);
    }

    private async Task<List<SearchRequestOption>> CreateOptionsAsync(IEnumerable<SearchRequestOptionDto> newOptions)
    {
        List<SearchRequestOption> createdOptions;
        if (newOptions.Any())
        {
            _logger.LogInformation(
                          "Creating new requests with '{0}' contentTypes, '{1}' websites and '{2} terms'",
                          newOptions.CommaSeparate(x => x.ContentType),
                          newOptions.CommaSeparate(x => x.Website),
                          newOptions.CommaSeparate(x => x.Term.ToString())
                          );
            createdOptions = newOptions
                .Select(newOption => new SearchRequestOption()
                {
                    ContentType = newOption.ContentType.Wrap(),
                    OccurredCount = 1,
                    SearchTerm = newOption.Term,
                    Website = newOption.Website.Wrap(),
                    Snapshots = new List<SearchOptionSnapshot>()
                })
                .ToList();
            await _context.Options
                .AddRangeAsync(createdOptions);
        }
        else
        {
            createdOptions = new();
        }
        return createdOptions;
    }

    public async Task MarkAsQueued(SearchRequestState request)
    {
        var items = request.StoredOptions.Values
            .Select(x => x.OptionId)
            .Select(optionId => new SearchOptionSnapshot
            {
                IsProcessed = false,
                Time = _dateTime.UtcNow,
                SearchOptionId = optionId
            });

        if (items.Any())
        {
            await using (var transaction = _context.Database.BeginTransaction())
            {
                await _context.Snapshots.AddRangeAsync(items);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
    }

    public async Task AddOrUpdateResults(SearchRequestState state, IEnumerable<SearchResultDto> results)
    {
        await using (var transaction = _context.Database.BeginTransaction())
        {
            await MarkAsProcessed(state);

            await StoreResults(state, results);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }

    private async Task MarkAsProcessed(SearchRequestState state)
    {
        var optionIds = state.StoredOptions.Values.Select(x => x.OptionId).ToList();
        await _context.Snapshots
            .Include(x => x.SearchOption)
            .Where(x => optionIds.Contains(x.SearchOptionId))
            .UpdateFromQueryAsync(obj =>
                new SearchOptionSnapshot()
                {
                    IsProcessed = true,
                    Time = obj.Time,
                    Id = obj.Id,
                    SearchOptionId = obj.SearchOptionId
                });
    }

    private async Task StoreResults(SearchRequestState state, IEnumerable<SearchResultDto> results)
    {
        var optionToSnapshot = state.StoredOptions
            .ToDictionary(option => option.Key, option => option.Value.Snapshots.Where(x => x.IsProcessed == false).LastOrDefault());

        var optionToItems = results
           .SelectMany(result => result.Items.Select(item => (item, option: new SearchRequestOptionDto(result.Website, item.ContentType, SearchOptionTerm.CreateAnd(result.Terms)))))
           .GroupBy(x => x.option)
           .ToDictionary(x => x.Key, x => x.Select(y => y.item).ToList());

        List<SearchResult> resultsToStore = new();
        foreach (var (option, items) in optionToItems)
        {
            if (optionToSnapshot.TryGetValue(option, out SearchSnapshot? snapshot) && snapshot is not null)
            {
                var snapshotId = snapshot.SnapshotId;
                foreach (var item in items)
                {
                    resultsToStore.Add(new SearchResult()
                    {
                        FoundTimeUtc = _dateTime.UtcNow,
                        ImagePreviewUrl = item.ImagePreviewUrl,
                        SearchItemUrl = item.SearchItemUrl,
                        AdditionalData = item.Data.ToJObject(),
                        SearchOptionSnapshotId = snapshotId
                    });
                }
            }
            else
            {
                //we are trying to store result that does not correspond with the request that produced it
                //silently ignore for now, we may return something here later
                _logger.LogWarning("Trying to store result that does not correspond to any request option");
            }
        }
        await _context.Result.AddRangeAsync(resultsToStore);

        _logger.LogInformation("Stored '{rowsCount}' rows of new search results", resultsToStore.Count);
    }
}
