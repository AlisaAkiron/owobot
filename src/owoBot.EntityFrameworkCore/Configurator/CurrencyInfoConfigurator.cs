using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using owoBot.Domain.Entities;

namespace owoBot.EntityFrameworkCore.Configurator;

public class CurrencyInfoConfigurator : IEntityTypeConfiguration<CurrencyInfo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CurrencyInfo> builder)
    {
        builder.HasKey(x => x.Code);
    }
}
