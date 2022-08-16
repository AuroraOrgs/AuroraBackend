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

    public TResult Resolve<TResult>(Func<T, TResult> onValue, Func<string, TResult> onNull)
    {
        TResult result;
        if (HasValue)
        {
            result = onValue(Value!);
        }
        else
        {
            result = onNull.Invoke(NullMessage ?? "");
        }
        return result;
    }

    public Task<TResult> ResolveAsync<TResult>(Func<T, Task<TResult>> onValue, Func<string, Task<TResult>> onNull)
    {
        Task<TResult> result;
        if (HasValue)
        {
            result = onValue(Value!);
        }
        else
        {
            result = onNull.Invoke(NullMessage ?? "");
        }
        return result;
    }

    public static ValueOrNull<T> CreateValue(T value)
    {
        return new ValueOrNull<T>
        {
            IsNull = false,
            Value = value
        };
    }

    public static ValueOrNull<T> CreateNull(string? nullMessage = null)
    {
        return new ValueOrNull<T>
        {
            IsNull = true,
            Value = default,
            NullMessage = nullMessage
        };
    }

    public static implicit operator ValueOrNull<T>(T value)
    {
        if (value is null)
        {
            return CreateNull();
        }
        return CreateValue(value);
    }
}

public static class ValueOrNullExtensions
{
    public static T WithDefault<T>(this ValueOrNull<T> valueOrNull, T defaultValue, Action<string>? onError = null)
    {
        return valueOrNull.Resolve<T>(x => x, msg =>
        {
            onError?.Invoke(msg);
            return defaultValue;
        });
    }
}
