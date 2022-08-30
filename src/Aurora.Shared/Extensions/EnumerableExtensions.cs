namespace Aurora.Shared.Extensions;

public static class EnumerableExtensions
{
    public static string CommaSeparate<T>(this IEnumerable<T> items) =>
        String.Join(", ", items.Select(x => x?.ToString() ?? ""));

    public static string CommaSeparate<T, U>(this IEnumerable<T> items, Func<T, U> transform) =>
        items.Select(transform).CommaSeparate();
}
