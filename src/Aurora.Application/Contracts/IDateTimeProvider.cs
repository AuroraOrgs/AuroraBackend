using System;

namespace Aurora.Application.Contracts
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
