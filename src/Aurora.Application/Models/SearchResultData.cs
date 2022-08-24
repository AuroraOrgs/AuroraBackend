using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aurora.Application.Models;

[JsonConverterAttribute(typeof(SearchDataConverter))]
public abstract record SearchResultData
{
}

//This is needed to override serialization of SearchResultData to object serialization, which is needed to preserve child fields
public class SearchDataConverter : JsonConverter<SearchResultData>
{
    public override SearchResultData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
       (SearchResultData?)JsonSerializer.Deserialize(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, SearchResultData value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, typeof(object), options);
}