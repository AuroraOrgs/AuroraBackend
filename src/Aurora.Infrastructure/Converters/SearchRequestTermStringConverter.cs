using Aurora.Application.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aurora.Infrastructure.Converters;

public class SearchRequestTermStringConverter : ValueConverter<SearchRequestTerm, string>
{
    public SearchRequestTermStringConverter() : base(term => FromTerm(term), str => FromString(str))
    {

    }

    private static SearchRequestTerm FromString(string str) =>
        SearchRequestTerm.ParseString(str);

    private static string FromTerm(SearchRequestTerm term) =>
        term.ToString();
}