using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Domain.Models.Fido;
using Ethachat.Auth.Domain.Models.WebPushNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ethachat.Auth.Infrastructure.DB.DBContext
{
    public class AuthContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AuthContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthContext(DbContextOptions<AuthContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserClaim> UsersClaim { get; set; }
        public DbSet<FidoUser> FidoUsers { get; set; }
        public DbSet<FidoCredential> StoredCredentials { get; set; }
        public DbSet<UserWebPushNotificationSubscription> WebPushNotificationSubscriptions { get; set; }
        public DbSet<UserAccessRefreshEventLog> UserAccessRefreshEventLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(GetConnectionString());
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        private string GetConnectionString()
        {
            var dbConnectionString = _configuration["ConnectionStrings:DbConnectionString"] ??
                                     throw new ArgumentException("Missing DB Connection string in dbConfiguration.json");

            var envDbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(envDbPassword))
                dbConnectionString = dbConnectionString.Replace("password", envDbPassword);

            return dbConnectionString;
        }
    }
}
