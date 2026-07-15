using System.Collections;

namespace Antigen.Resources.Converter;

public static class ObjectConverters
{
    public static string? GetStringValue(object? obj)
    {
        return obj switch
        {
            string s => s,
            IDictionary dictionary => string.Join("\n", dictionary.Keys.Cast<object>().Select(key => $"[{GetStringValue(key)}: {GetStringValue(dictionary[key])}]")),
            IEnumerable enumerable => string.Join("\n", enumerable.Cast<object?>().Select(GetStringValue)),
            _ => obj?.ToString()
        };
    }
}