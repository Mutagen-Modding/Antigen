using Antigen.Resources.Constants;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Antigen.Resources.Converter;

public static class SeverityConverters
{
    public static readonly FuncValueConverter<Severity, IBrush> ToRowLip = new(SeverityBrushes.RowLip);
    public static readonly FuncValueConverter<Severity, IBrush> ToRowFill = new(SeverityBrushes.RowFill);
    public static readonly FuncValueConverter<Severity, IBrush> ToRowHoverStroke = new(SeverityBrushes.RowHoverStroke);
    public static readonly FuncValueConverter<Severity, IBrush> ToRowHoverFill = new(SeverityBrushes.RowHoverFill);

    public static readonly FuncValueConverter<Severity, string> ToDescription = new(severity =>
    {
        return severity switch
        {
            Severity.None => "Nitpick, ignored by default",
            Severity.Suggestion => "Best practice, but not a problem",
            Severity.Warning => "Something is weird, but not necessarily a problem",
            Severity.Error => "Will cause bugs, but won't necessarily crash the game",
            Severity.CTD => "Will cause a CTD",
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    });
}