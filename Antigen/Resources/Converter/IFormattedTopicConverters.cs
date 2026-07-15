using Avalonia.Data.Converters;

namespace Antigen.Resources.Converter;

public interface IFormattedTopicConverters
{
    IValueConverter ExtractMessage { get; }
}