using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using owoBot.Domain.Entities;

namespace owoBot.EntityFrameworkCore;

// ReSharper disable once InconsistentNaming
[AutoConstructor]
public partial class OWODbContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public DbSet<CurrencyInfo> CurrencyInfos => Set<CurrencyInfo>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var connectionString = _configuration.GetConnectionString("PostgreSQL");

        optionsBuilder.UseNpgsql(connectionString);

        if (_hostEnvironment.IsProduction() is false)
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
