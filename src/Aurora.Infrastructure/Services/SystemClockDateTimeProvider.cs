using Aurora.Application.Contracts;

namespace Aurora.Infrastructure.Services;

public class SystemClockDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
