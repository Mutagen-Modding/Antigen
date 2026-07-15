using Avalonia.Data.Converters;
using Noggog;

namespace Antigen.Resources.Converter;

public static class RecordTypeConverters
{
    public static IValueConverter GetName { get; } = new FuncValueConverter<Type, string>(type => type?.Name
        .TrimStart("I", StringComparison.OrdinalIgnoreCase)
        .TrimStringFromEnd("Getter")
        .ToString() ?? string.Empty);
}