using Aurora.Shared.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aurora.Infrastructure.Services;

public class EnumWrapperConverter<T> : ValueConverter<EnumWrapper<T>, T> where T : Enum
{
    public EnumWrapperConverter() : base(
        v => v.Value,
        v => Create(v)
        )
    {
    }

    private static EnumWrapper<T> Create(T value) =>
        EnumWrapper<T>.Create(value)
                      .Resolve(
                            value => value,
                            err => throw new Exception($"Failed to create enum: {err}")
                            );
}
