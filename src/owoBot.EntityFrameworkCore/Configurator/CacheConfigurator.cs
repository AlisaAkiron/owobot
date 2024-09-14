using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using owoBot.Domain.Entities;

namespace owoBot.EntityFrameworkCore.Configurator;

public class CacheConfigurator : IEntityTypeConfiguration<Cache>
{
    public void Configure(EntityTypeBuilder<Cache> builder)
    {
        builder.HasKey(x => x.Key);
    }
}
