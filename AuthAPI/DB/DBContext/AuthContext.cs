using AuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DB.DBContext
{
    public class AuthContext : DbContext
    {
        private const string CONNECTION_STRING = "Host=localhost:9000;Username=postgres;Password=password;Database=postgres";
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(CONNECTION_STRING);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
