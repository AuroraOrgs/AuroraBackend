namespace Aurora.Shared.Models;

public struct ValueOrNull<T>
{
    public bool IsNull { get; set; }
    private T? Value { get; set; }

    public bool HasValue => !IsNull;
    public string? NullMessage { get; set; }

    public void Resolve(Action<T> onValue, Action<string>? onNull = null)
    {
        if (HasValue)
        {
            onValue(Value!);
        }
        else
        {
            onNull?.Invoke(NullMessage ?? "");
        }
    }

    public TResult Resolve<TResult>(Func<T, TResult> onValue, Func<string, TResult> onNull) =>
        HasValue
            ? onValue(Value!)
            : onNull.Invoke(NullMessage ?? "");

    public Task ResolveAsync(Func<T, Task> onValue, Func<string, Task> onNull) =>
        HasValue
            ? onValue(Value!)
            : onNull.Invoke(NullMessage ?? "");

    public Task<TResult> ResolveAsync<TResult>(Func<T, Task<TResult>> onValue, Func<string, Task<TResult>> onNull) =>
        HasValue
            ? onValue(Value!)
            : onNull.Invoke(NullMessage ?? "");

    public static ValueOrNull<T> CreateValue(T value) =>
        new ValueOrNull<T>
        {
            IsNull = false,
            Value = value
        };

    public static ValueOrNull<T> CreateNull(string? nullMessage = null) =>
        new ValueOrNull<T>
        {
            IsNull = true,
            Value = default,
            NullMessage = nullMessage
        };

    public static implicit operator ValueOrNull<T>(T value) =>
        value is null
            ? CreateNull()
            : CreateValue(value);
}

public static class ValueOrNullExtensions
{
    public static ValueOrNull<U> PipeValue<T, U>(this ValueOrNull<T> valueOrNull, Func<T, U> transform) =>
        valueOrNull.Resolve(
            value => transform(value).ToResult(),
            errorMessage => errorMessage.ToErrorResult<U>()
            );

    public static ValueOrNull<U> PipeValue<T, U>(this ValueOrNull<T> valueOrNull, Func<T, ValueOrNull<U>> transform) =>
        valueOrNull.Resolve(
            value => transform(value),
            errorMessage => errorMessage.ToErrorResult<U>()
            );

    public static T WithDefault<T>(this ValueOrNull<T> valueOrNull, T defaultValue, Action<string>? onError = null) =>
        valueOrNull.Resolve<T>(x => x, msg =>
        {
            onError?.Invoke(msg);
            return defaultValue;
        });

    public static ValueOrNull<T> ToErrorResult<T>(this string errorMessage) =>
        ValueOrNull<T>.CreateNull(errorMessage);

    public static ValueOrNull<T> ToResult<T>(this T result) =>
        ValueOrNull<T>.CreateValue(result);
}
