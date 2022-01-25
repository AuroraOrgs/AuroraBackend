using Aurora.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aurora.Infrastructure
{
    public class SearchContext : DbContext
    {
        public SearchContext(DbContextOptions<SearchContext> options) : base(options) { }

        public DbSet<SearchRequest> Request { get; set; }
        public DbSet<SearchResult> Result { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SearchRequest>()
               .HasIndex(p => new { p.SearchTerm, p.Website, p.ContentOption }).IsUnique();
        }
    }
}