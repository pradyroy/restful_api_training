using Microsoft.Extensions.DependencyInjection;
using Users.Application.Users.Services;
using Users.Application.Auth;

namespace Users.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application layer services
        services.AddScoped<UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
