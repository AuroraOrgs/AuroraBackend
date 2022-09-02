using Aurora.Domain.Entities;
using Aurora.Domain.ValueObjects;
using Aurora.Infrastructure.Extensions;
using Aurora.Infrastructure.Services;
using Aurora.Shared.Extensions;
using Aurora.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aurora.Infrastructure;

public class SearchContext : DbContext
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SearchContext(DbContextOptions<SearchContext> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public DbSet<SearchRequestOption> Options { get; set; }
    public DbSet<SearchResult> Result { get; set; }
    public DbSet<SearchOptionSnapshot> Snapshots { get; internal set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchRequestOption>()
           .HasIndex(p => new { p.SearchTerm, p.Website, p.ContentType }).IsUnique();

        modelBuilder.Entity<SearchResult>()
            .Property(p => p.AdditionalData)
            .HasJsonConversion();

        ValueConverter<SearchOptionTerm, string> termConverter = new(v => v.ToString(), v => SearchOptionTerm.ParseString(v));

        modelBuilder.Entity<SearchRequestOption>()
            .Property(x => x.SearchTerm)
            .HasConversion(termConverter);

        AddEnumWrapperConversions(modelBuilder);
    }

    private static void AddEnumWrapperConversions(ModelBuilder modelBuilder)
    {
        var wrapperBaseType = typeof(EnumWrapper<>);
        var enumProperties = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(x => x.GetProperties())
            .Where(x => x.ClrType.IsAssignableToGenericType(wrapperBaseType));

        var numberConverterBaseType = typeof(EnumToNumberConverter<,>);
        var enumNumberType = typeof(int);
        var convererBaseType = typeof(EnumWrapperConverter<>);
        foreach (var property in enumProperties)
        {
            var enumType = property.ClrType.GenericTypeArguments[0];
            var numberConverterType = numberConverterBaseType.MakeGenericType(new[] { enumType, enumNumberType });
            var numberConverter = (ValueConverter)Activator.CreateInstance(numberConverterType)!;
            var converterType = convererBaseType.MakeGenericType(new[] { enumType });
            var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
            //convert wrapper to enum then to int
            var finalConverter = converter.ComposeWith(numberConverter);
            property.SetValueConverter(finalConverter);
        }
    }
}