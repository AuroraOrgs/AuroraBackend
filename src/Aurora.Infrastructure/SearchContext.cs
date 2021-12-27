using Aurora.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aurora.Infrastructure
{
    public class SearchContext : DbContext
    {
        public SearchContext(DbContextOptions<SearchContext> options) : base(options) { }

        public DbSet<SearchRequest> Request { get; set; }
        public DbSet<SearchResult> Result { get; set; }
    }
}
