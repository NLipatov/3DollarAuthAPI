using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ethachat.Auth.Infrastructure.Context;

public static class ContextExtensions
{
    public static async Task ApplyPendingMigrationsAsync<TDbContext>(this AsyncServiceScope scope)
        where TDbContext : DbContext
    {
        await using TDbContext context = scope.ServiceProvider
            .GetRequiredService<TDbContext>();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await context.Database.MigrateAsync();
        }
    }
}