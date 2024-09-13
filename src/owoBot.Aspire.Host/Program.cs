using owoBot.Aspire.Host;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.ConfigureAppHost();

#region Parameters

var postgresqlPassword = builder.AddParameter("postgresql-password", true);
var postgresqlTag = builder.AddParameter("postgresql-tag");

var enablePgadmin = builder.AddParameter("enable-pgadmin");

#endregion

#region External Services

var postgresql = builder
    .AddResourceWithConnectionString(b =>
    {
        var pg = b
            .AddPostgres("postgresql-instance", password: postgresqlPassword)
            .WithOtlpExporter()
            .WithImageTag(postgresqlTag.Resource.Value)
            .WithDataVolume("owobot");
        if (enablePgadmin.Resource.Value == "true")
        {
            pg.WithPgAdmin(pgadmin => pgadmin.WithImageTag("latest"));
        }

        pg.AddDatabase("owobot");
        return pg;
    }, "PostgreSQL");

#endregion

builder
    .AddProject<owoBot_App_Bot>("bot")
    .WithReference(postgresql);

await builder.Build().RunAsync();
