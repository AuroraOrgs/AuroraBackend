using Aurora.Application.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Infrastructure.Extensions
{
    public static class ValueConversionExtensions
    {
        private static JsonSerializerSettings _jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public static PropertyBuilder<JObject?> HasJsonConversion(this PropertyBuilder<JObject?> propertyBuilder)
        {
            ValueConverter<JObject?, string> converter = new(v => v.ConvertToString(), v => v.ParseNullableJson());

            ValueComparer<JObject?> comparer = new(
                (l, r) => JToken.DeepEquals(l, r),
                v => v == null ? 0 : v!.ToString().GetHashCode(),
                v => v.ConvertToString().ParseNullableJson()
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }

        public static SearchResultData? GetData(this JObject? obj)
        {
            SearchResultData? result;
            if (obj is null)
            {
                result = null;
            }
            else
            {

                var str = obj.ToString();
                result = JsonConvert.DeserializeObject<SearchResultData>(str, _jsonSettings);
            }
            return result;
        }

        internal static JObject? ParseNullableJson(this string str)
        {
            JObject? result;
            try
            {
                var obj = JsonConvert.DeserializeObject(str, _jsonSettings);
                if (obj is not null)
                {
                    var serializer = JsonSerializer.Create(_jsonSettings);
                    result = JObject.FromObject(obj, serializer);
                }
                else
                {
                    result = null;
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        internal static string ConvertToString(this JObject? v)
        {
            return v is not null
                ? JsonConvert.SerializeObject(v, _jsonSettings)
                : "";
        }
    }
}
