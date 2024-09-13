using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using owoBot.Domain.Entities;

namespace owoBot.EntityFrameworkCore.Configurator;

public class ExchangeRateConfigurator : IEntityTypeConfiguration<ExchangeRate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(x => new
        {
            x.Source, x.Target
        });
    }
}
