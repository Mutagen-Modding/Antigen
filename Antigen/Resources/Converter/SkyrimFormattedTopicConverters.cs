using System.Reflection;
using System.Text.RegularExpressions;
using Antigen.Resources.Constants;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using ReactiveUI;

namespace Antigen.Resources.Converter;

public sealed partial class SkyrimFormattedTopicConverters : IFormattedTopicConverters
{
    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderRegex { get; }

    public IValueConverter ExtractMessage { get; } = new FuncValueConverter<IFormattedTopicDefinition?, object?>(ExtractTopicFormat);
    private static TextBlock? ExtractTopicFormat(IFormattedTopicDefinition? formattedTopicDefinition)
    {
        if (formattedTopicDefinition is null) return null;

        var textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 10,
            Foreground = StandardBrushes.DarkGrayBrush
        };
        textBlock.Inlines ??= new InlineCollection();

        var topicItems = formattedTopicDefinition.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.StartsWith("Item", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .Select(x => x.GetValue(formattedTopicDefinition))
            .ToArray();

        var textParts = PlaceholderRegex.Split(formattedTopicDefinition.TopicDefinition.MessageFormat);
        for (var i = 0; i < textParts.Length; i++)
        {
            var textPart = textParts[i];

            textBlock.Inlines.Add(new Run
            {
                Text = textPart,
                FontSize = 10,
                BaselineAlignment = BaselineAlignment.Center
            });

            if (topicItems.Length <= i) continue;

            var item = topicItems[i];
            item = item switch
            {
                IMajorRecordIdentifierGetter recordIdentifier => recordIdentifier.EditorID ?? recordIdentifier.FormKey.ToString(),
                IFormLinkIdentifier formLinkIdentifier => ObjectConverters.GetStringValue(formLinkIdentifier),
                IConditionFloatGetter condition => $"Condition: {condition.Data.RunOnType}.{condition.Data.Function} {condition.CompareOperator} {condition.ComparisonValue}",
                IConditionGlobalGetter condition => $"Condition: {condition.Data.RunOnType}.{condition.Data.Function} {condition.CompareOperator} Global={condition.ComparisonValue.FormKey}",
                _ => item
            };

            textBlock.Inlines.Add(new InlineUIContainer(new Button
            {
                Content = item?.ToString(),
                FontSize = 10,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = StandardBrushes.TextBrush,
                Margin = new Thickness(0),
                Padding = new Thickness(0, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Command = ReactiveCommand.Create<string?>(param =>
                {
                    if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow.Clipboard: {} clipboard }) return;

                    clipboard.SetTextAsync(param);
                }),
                CommandParameter = item?.ToString(),
                [ToolTip.TipProperty] = "Copy"
            }));
        }

        return textBlock;
    }
}