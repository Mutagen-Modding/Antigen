using System.Collections;

namespace Antigen.Reporting.Handlers;

public static class ReportUtility
{
    public static string? GetStringValue(object? obj)
    {
        return obj switch
        {
            string s => s,
            IDictionary dictionary => string.Join(", ", dictionary.Keys.Cast<object>().Select(key => $"[{GetStringValue(key)}: {GetStringValue(dictionary[key])}]")),
            IEnumerable enumerable => string.Join(", ", enumerable.Cast<object?>().Select(GetStringValue)),
            _ => obj?.ToString()
        };
    }
}
