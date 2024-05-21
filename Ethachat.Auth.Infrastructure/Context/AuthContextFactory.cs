using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ethachat.Auth.Infrastructure.DB.DBContext
{
    /// <summary>
    /// Used for CLI ef commands, such as `dotnet ef database update`
    /// </summary>
    public class AuthContextFactory : IDesignTimeDbContextFactory<AuthContext>
    {
        public AuthContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("dbConfiguration.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DbConnectionString"));

            return new AuthContext(configuration);
        }
    }
}