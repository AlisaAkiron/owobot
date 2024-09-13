using owoBot.App.Bot;
using owoBot.App.Bot.Extensions;
using owoBot.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddDefaultServices();
builder.AddBotServices();

var app = builder.Build();

await app.MigrateDatabaseAsync();

await app.RunAsync();
