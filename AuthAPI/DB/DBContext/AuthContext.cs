using AuthAPI.DB.Models;
using AuthAPI.DB.Models.Fido;
using AuthAPI.DB.Models.WebPushNotifications;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DB.DBContext
{
    public class AuthContext : DbContext
    {
        public AuthContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConfiguration _configuration;

        public DbSet<User> Users { get; set; }
        public DbSet<UserClaim> UsersClaim { get; set; }
        public DbSet<FidoUser> FidoUsers { get; set; }
        public DbSet<FidoCredential> StoredCredentials { get; set; }
        public DbSet<UserWebPushNotificationSubscription> WebPushNotificationSubscriptions { get; set; }
        public DbSet<UserAccessRefreshEventLog> UserAccessRefreshEventLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(GetConnectionString());
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        private string GetConnectionString()
        {
            var dbConnectionString = _configuration["ConnectionStrings:PostgreSQL_DEV"] ??
                                     throw new ArgumentException("Missing DB Connection string in appsettings.json");

            var envDbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            
            if (!string.IsNullOrWhiteSpace(envDbPassword))
                dbConnectionString = dbConnectionString.Replace("password", envDbPassword);

            return dbConnectionString;
        }
    }
}