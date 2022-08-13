using Newtonsoft.Json.Linq;

namespace Aurora.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static JObject? ParseNullableJson(this string str)
        {
            JObject? result;
            try
            {
                result = JObject.Parse(str);
            }
            catch
            {
                result = null;
            }
            return result;
        }
    }
}
