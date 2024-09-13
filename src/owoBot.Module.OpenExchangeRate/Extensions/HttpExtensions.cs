namespace owoBot.Module.OpenExchangeRate.Extensions;

public static class HttpExtensions
{
    public static string AddQuery(this string uri, string key, object value)
    {
        var separator = uri.Contains('?') ? "&" : "?";
        return $"{uri}{separator}{key}={value}";
    }
}
