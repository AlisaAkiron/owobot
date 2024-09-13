using Microsoft.EntityFrameworkCore;
using owoBot.Domain.Entities;

namespace owoBot.EntityFrameworkCore;

// ReSharper disable once InconsistentNaming
public class OWODbContext : DbContext
{
    public OWODbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CurrencyInfo> CurrencyInfos => Set<CurrencyInfo>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OWODbContext).Assembly);
    }
}
