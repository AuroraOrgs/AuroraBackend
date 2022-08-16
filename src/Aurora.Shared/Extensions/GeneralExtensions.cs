namespace Aurora.Shared.Extensions;

public static class GeneralExtensions
{
    public static bool IsEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNotEmpty(this string str)
    {
        return !str.IsEmpty();
    }

    public static bool None<T>(this IEnumerable<T> collection)
    {
        return !collection.Any();
    }

    public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        return !collection.Any(predicate);
    }

    public static bool NotContains<T>(this IEnumerable<T> source, T value)
    {
        return !source.Contains(value);
    }

    public static bool NotEndsWith(this string str, string endStr)
    {
        return !str.EndsWith(endStr);
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
    {
        TValue res;
        if (dict.TryGetValue(key, out res) == false)
        {
            res = defaultValue;
        }
        return res;
    }
}