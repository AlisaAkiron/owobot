namespace owoBot.Domain.Extensions;

public static class LinqExtensions
{
    public static string JoinAsString(this IEnumerable<string> source, string separator)
    {
        return string.Join(separator, source);
    }
}
