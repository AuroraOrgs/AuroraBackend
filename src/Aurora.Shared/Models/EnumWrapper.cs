namespace Aurora.Shared.Models;

public class EnumWrapper<T> where T : Enum
{
    public T Value { get; }
    private EnumWrapper(T value)
    {
        Value = value;
    }

    public static ValueOrNull<EnumWrapper<T>> Create(T value)
    {
        ValueOrNull<EnumWrapper<T>> result;
        if (Enum.IsDefined(typeof(T), value))
        {
            result = new EnumWrapper<T>(value);
        }
        else
        {
            result = $"Value '{value}' is not defined for enum of type '{typeof(T)}'".ToErrorResult<EnumWrapper<T>>();
        }
        return result;
    }

    public static implicit operator T(EnumWrapper<T> wrapper) => wrapper.Value;
}

public static class EnumWrapperExtensions
{
    public static EnumWrapper<T> Wrap<T>(this T value) where T : Enum =>
        EnumWrapper<T>.Create(value).Resolve(value => value, err => throw new Exception(err));
}