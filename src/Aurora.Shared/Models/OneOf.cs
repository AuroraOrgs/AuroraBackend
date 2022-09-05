using Aurora.Shared.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aurora.Shared.Models;

public class OneOf<T0, T1>
{
    private readonly bool _isT0;
    // It is better to store both fields instead of having an object field
    // because of boxing when assigning and casting when using
    private readonly T0 _t0;
    private readonly T1 _t1;

    public OneOf(T0 value)
    {
        _t0 = value;
        _t1 = default!;
        _isT0 = true;
    }

    public OneOf(T1 value)
    {
        _t0 = default!;
        _t1 = value;
        _isT0 = false;
    }

    public TResult Match<TResult>(Func<T0, TResult> func0, Func<T1, TResult> func1) =>
        _isT0
            ? func0(_t0)
            : func1(_t1);

    public void Match(Action<T0> func0, Action<T1> func1)
    {
        if (_isT0)
        {
            func0(_t0);
        }
        else
        {
            func1(_t1);
        }
    }

    public Task MatchAsync(Func<T0, Task> func0, Func<T1, Task> func1) =>
          _isT0
            ? func0(_t0)
            : func1(_t1);
}

public static class OneOfExtensions
{
    public static Task MatchAsync<T0, T1>(this OneOf<T0, T1> oneOf, Func<T0, Task> func0, Func<T1, Task> func1) =>
          oneOf.Match(func0, func1);

    public static Task<TResult> MatchAsync<T0, T1, TResult>(this OneOf<T0, T1> oneOf, Func<T0, Task<TResult>> func0, Func<T1, Task<TResult>> func1) =>
          oneOf.Match(func0, func1);
}

public class OneOfJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAssignableToGenericType(typeof(OneOf<,>));

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        JsonConverter? converter;
        if (typeToConvert.IsAssignableToGenericType(typeof(OneOf<,>)))
        {
            Type firstType = typeToConvert.GetGenericArguments()[0];
            Type secondType = typeToConvert.GetGenericArguments()[1];

            converter = (JsonConverter)Activator.CreateInstance(
                typeof(OneOfConverter<,>).MakeGenericType(
                    new Type[] { firstType, secondType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: Array.Empty<object>(),
                culture: null)!;
        }
        else
        {
            converter = null;
        }
        return converter;
    }

    private class OneOfConverter<T0, T1> : JsonConverter<OneOf<T0, T1>>
    {
        public override OneOf<T0, T1>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var first = (T0?)JsonSerializer.Deserialize(ref reader, typeof(T0), options);
                if (first is not null)
                {
                    return new(first);
                }
            }
            catch
            {
            }

            try
            {
                var second = (T1?)JsonSerializer.Deserialize(ref reader, typeof(T1), options);
                if (second is not null)
                {
                    return new(second);
                }
            }
            catch
            {
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, OneOf<T0, T1> value, JsonSerializerOptions options)
        {
            value.Match(
                x =>
                {
                    JsonSerializer.Serialize(writer, x, typeof(T0), options);
                },
                x =>
                {
                    JsonSerializer.Serialize(writer, x, typeof(T1), options);
                });
        }
    }
}