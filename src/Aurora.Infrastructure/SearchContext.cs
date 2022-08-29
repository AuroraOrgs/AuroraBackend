using Aurora.Application.Entities;
using Aurora.Infrastructure.Converters;
using Aurora.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Aurora.Infrastructure;

public class SearchContext : DbContext
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SearchContext(DbContextOptions<SearchContext> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public DbSet<SearchRequest> Request { get; set; }
    public DbSet<SearchResult> Result { get; set; }
    public DbSet<SearchRequestQueueItem> Queue { get; set; }
    public DbSet<SearchRequestSnapshot> Snapshots { get; internal set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchRequest>()
           .HasIndex(p => new { p.SearchTerm, p.Website, p.ContentType }).IsUnique();

        modelBuilder.Entity<SearchResult>()
            .Property(p => p.AdditionalData)
            .HasJsonConversion();

        modelBuilder.Entity<SearchRequest>()
            .Property(x => x.SearchTerm)
            .HasConversion<SearchRequestTermStringConverter>();
    }
}