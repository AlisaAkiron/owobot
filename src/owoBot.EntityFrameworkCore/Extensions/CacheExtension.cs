using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using owoBot.Domain.Entities;

namespace owoBot.EntityFrameworkCore.Extensions;

public static class CacheExtension
{
    public static async Task<JsonDocument?> GetValueAsync(this DbSet<Cache> dbSet, string key)
    {
        var cache = await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key);
        return cache?.Value;
    }

    public static async Task<T?> GetValueAsync<T>(this DbSet<Cache> dbSet, string key) where T : class
    {
        var cache = await dbSet.GetValueAsync(key);
        return cache?.Deserialize<T>();
    }

    public static async Task<string> GetStringAsync(this DbSet<Cache> dbSet, string key)
    {
        var value = await dbSet.GetValueAsync<StringValue>(key);
        return value?.Value ?? string.Empty;
    }

    public static async Task SetValue(this DbSet<Cache> dbSet, string key, JsonDocument value)
    {
        var cache = await dbSet.FirstOrDefaultAsync(x => x.Key == key);
        if (cache is null)
        {
            await dbSet.AddAsync(new Cache
            {
                Key = key,
                Value = value
            });
        }
        else
        {
            cache.Value = value;
        }
    }

    public static async Task SetValue<T>(this DbSet<Cache> dbSet, string key, T value) where T : class
    {
        await dbSet.SetValue(key, JsonSerializer.SerializeToDocument(value));
    }

    public static async Task SetStringAsync(this DbSet<Cache> dbSet, string key, string value)
    {
        await dbSet.SetValue(key, new StringValue(value));
    }

    private sealed record StringValue(string Value);
}
