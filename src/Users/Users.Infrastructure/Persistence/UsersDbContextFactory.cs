using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Users.Infrastructure.Persistence;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();

        // Design-time connection string (for migrations only)
        var connectionString =
            "Server=localhost;Port=3306;Database=restful_api_training;User=root;Password=pa55word;";

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new UsersDbContext(optionsBuilder.Options);
    }
}
