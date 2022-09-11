using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aurora.Domain.ValueObjects;

[JsonConverter(typeof(SearchOptionTermConverter))]
public class SearchOptionTerm : ValueObject
{
    private readonly IEnumerable<string> _terms;
    private SearchOptionTerm(IEnumerable<string> terms)
    {
        //Guarantee order for value comparison
        _terms = terms.OrderBy(x => x);
    }

    public IEnumerable<string> Terms => _terms;

    public static SearchOptionTerm CreateAnd(IEnumerable<string> terms) =>
        new(terms);

    public static SearchOptionTerm ParseString(string str) =>
        new(str.Split(','));

    public override string ToString() =>
        string.Join(',', _terms.Distinct());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var term in _terms)
        {
            yield return term;
        }
    }
}

public class SearchOptionTermConverter : JsonConverter<SearchOptionTerm>
{
    public override SearchOptionTerm? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (SearchOptionTerm?)JsonSerializer.Deserialize(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, SearchOptionTerm value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
    }
}