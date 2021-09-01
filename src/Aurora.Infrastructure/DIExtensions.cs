using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure
{
    public static class DIExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            //register all of the services
            return services;
        }
    }
}
