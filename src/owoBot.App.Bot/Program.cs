using owoBot.App.Bot;
using owoBot.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddDefaultServices();
builder.AddBotServices();

var app = builder.Build();

app.Run();
