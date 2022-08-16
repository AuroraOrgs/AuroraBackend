using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Shared.Extensions;

public static class JsonExtensions
{
    private static JsonSerializerSettings _jsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public static JObject? ToJObject<T>(this T? obj)
    {
        JObject? result;
        if (obj is not null)
        {
            var converter = JsonSerializer.Create(_jsonSettings);
            result = JObject.FromObject(obj, converter);
        }
        else
        {
            result = null;
        }
        return result;
    }

    public static T? ToData<T>(this JObject? obj)
    {
        T? result;
        if (obj is null)
        {
            result = default(T);
        }
        else
        {
            var str = obj.ToString();
            result = JsonConvert.DeserializeObject<T>(str, _jsonSettings);
        }
        return result;
    }

    public static JObject? ParseNullableJson(this string str)
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

    public static string ConvertToString(this JObject? v)
    {
        return v is not null
            ? JsonConvert.SerializeObject(v, _jsonSettings)
            : "";
    }
}
