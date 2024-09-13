using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace owoBot.Aspire.Host;

public static class Extension
{
    public static IDistributedApplicationBuilder ConfigureAppHost(this IDistributedApplicationBuilder builder)
    {
        builder.ConfigureSerilog();

        return builder;
    }

    public static IResourceBuilder<IResourceWithConnectionString> AddResourceWithConnectionString(
        this IDistributedApplicationBuilder builder,
        Func<IDistributedApplicationBuilder, IResourceBuilder<IResourceWithConnectionString>> resourceBuilder,
        string connectionStringName)
    {
        var csValue = builder.Configuration.GetConnectionString(connectionStringName);
        return string.IsNullOrEmpty(csValue)
            ? resourceBuilder.Invoke(builder)
            : builder.AddConnectionString(connectionStringName);
    }

    public static IResourceBuilder<IResourceWithConnectionString> AddConnectionStringWithDefault(
        this IDistributedApplicationBuilder builder,
        string connectionStringName,
        string defaultValue)
    {
        var csValue = builder.Configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrEmpty(csValue))
        {
            builder.Configuration[connectionStringName] = defaultValue;
        }

        return builder.AddConnectionString(connectionStringName);
    }

    private static IDistributedApplicationBuilder ConfigureSerilog(this IDistributedApplicationBuilder builder)
    {
        builder.Services.AddSerilog(cfg =>
        {
            cfg
                .WriteTo.Console()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            if (builder.Environment.IsProduction())
            {
                cfg.MinimumLevel.Override("Aspire.Hosting.Dcp", LogEventLevel.Warning);
            }
        });

        return builder;
    }
}
