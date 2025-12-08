using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Users.Application.Contracts;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
