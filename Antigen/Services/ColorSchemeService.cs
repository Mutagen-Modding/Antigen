using Antigen.Models.Settings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FluentAvalonia.Styling;

namespace Antigen.Services;

public sealed class ColorSchemeService : ISingleton
{
    public const ColorScheme Default = ColorScheme.Antigen;

    private static readonly Uri BaseUri = new("avares://Antigen/App.axaml");

    private static readonly string[] AccentPinnedKeys =
    [
        "AccentFillColorDefaultBrush", "AccentFillColorSecondaryBrush", "AccentFillColorTertiaryBrush",
        "SliderTrackValueFill", "SliderTrackValueFillPointerOver", "SliderTrackValueFillPressed",
        "SliderThumbBackground",
        "ComboBoxItemPillFillBrush",
    ];

    private static readonly string[] HoverPinnedKeys =
    [
        "SliderThumbBackgroundPointerOver", "SliderThumbBackgroundPressed",
        "ComboBoxItemForegroundPointerOver", "ComboBoxItemForegroundSelectedPointerOver",
    ];

    private IResourceProvider? _applied;

    public ColorScheme Current { get; private set; } = Default;

    public void Apply(ColorScheme scheme)
    {
        if (Application.Current is not { } app) return;

        Current = scheme;

        var dictionaries = app.Resources.MergedDictionaries;
        if (_applied is not null)
        {
            dictionaries.Remove(_applied);
        }

        _applied = new ResourceInclude(BaseUri)
        {
            Source = new Uri($"avares://Antigen/Resources/Themes/{scheme}.axaml")
        };
        dictionaries.Add(_applied);

        ApplyFluentAccent(app);
    }

    private static void ApplyFluentAccent(Application app)
    {
        if (app.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault() is not { } fluent) return;
        if (app.TryFindResource("AccentColor", app.ActualThemeVariant, out var accent) && accent is Color color)
        {
            fluent.CustomAccentColor = color;

            var brush = new ImmutableSolidColorBrush(color);
            foreach (var key in AccentPinnedKeys)
            {
                app.Resources[key] = brush;
            }
        }

        if (app.TryFindResource("GlyphHoverBrush", app.ActualThemeVariant, out var hover) && hover is IBrush hoverBrush)
        {
            foreach (var key in HoverPinnedKeys)
            {
                app.Resources[key] = hoverBrush;
            }
        }
    }
}
