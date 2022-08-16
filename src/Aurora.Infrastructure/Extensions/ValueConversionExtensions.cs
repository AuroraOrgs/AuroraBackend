using Aurora.Shared.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;

namespace Aurora.Infrastructure.Extensions;

public static class ValueConversionExtensions
{
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
}
