using Microsoft.EntityFrameworkCore;
using owoBot.EntityFrameworkCore;

namespace owoBot.App.Bot.Extensions;

public static class ApplicationExtensions
{
    public static async Task MigrateDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<OWODbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<OWODbContext>>();

        var migrations = dbContext.Database.GetPendingMigrations().ToList();

        if (migrations.Count == 0)
        {
            return;
        }

        await dbContext.Database.MigrateAsync();

        foreach (var migration in migrations)
        {
            logger.LogInformation("Migrated database: {Migration}", migration);
        }
    }
}
