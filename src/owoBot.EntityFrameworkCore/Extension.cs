using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace owoBot.EntityFrameworkCore;

public static class Extension
{
    public static void AddEntityFrameworkCore(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<OWODbContext>();
    }
}
