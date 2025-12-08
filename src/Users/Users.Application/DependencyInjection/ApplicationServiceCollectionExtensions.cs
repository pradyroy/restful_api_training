using Microsoft.Extensions.DependencyInjection;
using Users.Application.Users.Services;

namespace Users.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application layer services
        services.AddScoped<UserService>();

        return services;
    }
}
