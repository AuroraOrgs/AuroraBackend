using Aurora.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aurora.Infrastructure.Converters;

public class SearchRequestTermStringConverter : ValueConverter<SearchOptionTerm, string>
{
    public SearchRequestTermStringConverter() : base(term => FromTerm(term), str => FromString(str))
    {

    }

    private static SearchOptionTerm FromString(string str) =>
        SearchOptionTerm.ParseString(str);

    private static string FromTerm(SearchOptionTerm term) =>
        term.ToString();
}