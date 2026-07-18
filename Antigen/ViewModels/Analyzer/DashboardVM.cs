using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Antigen.Extensions;
using Antigen.Models;
using Antigen.Resources.Command;
using Antigen.Resources.Constants;
using Antigen.Resources.Converter;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Noggog;
using ReactiveUI.SourceGenerators;
using Sortable.Avalonia;

namespace Antigen.ViewModels.Analyzer;

public sealed partial class DashboardVM : ViewModel
{
    private readonly IFormattedTopicConverters _formattedTopicConverters;

    public AnalyzerVM AnalyzerVM { get; }
    public ModWatcherVM ModWatcher => AnalyzerVM.ModWatcher;

    [Reactive] public partial HierarchicalTreeDataGridSource<object>? ResultsTreeSource { get; set; }

    public ObservableCollectionExtended<Grouping<AnalyzerResultVM>> ActiveGroupings { get; } = [];
    public ObservableCollectionExtended<Grouping<AnalyzerResultVM>> InactiveGroupings { get; } = [];
    public IReadOnlyList<Grouping<AnalyzerResultVM>> AllGroupings { get; } =
    [
        new("Severity", result => result.Result.Topic.Severity.ToString()),
        new("Topic Title", result => result.Result.Topic.TopicDefinition.Title ?? string.Empty),
        new("Record Type", result => RecordTypeConverters.GetName.Convert(result.Result.Record?.Type, typeof(string), null!, null!) as string ?? string.Empty),
        new("Record", result => result.RecordDisplayName ?? string.Empty),
        new("Parent Record", result => result.ParentDisplayName ?? string.Empty),
    ];

    public ReadOnlyObservableCollection<AnalyzerResultVM> FilteredResults => AnalyzerVM.FilteredResults;
    public ObservableCollectionExtended<Severity> EnabledSeverities => AnalyzerVM.EnabledSeverities;

    public DashboardVM(
        AnalyzerVM analyzerVM,
        IFormattedTopicConverters formattedTopicConverters,
        ILogger<DashboardVM> logger)
    {
        AnalyzerVM = analyzerVM;
        _formattedTopicConverters = formattedTopicConverters;

        ActiveGroupings.ObserveCollectionChanges()
            .Subscribe(UpdateResultsTreeSource)
            .DisposeWith(this);

        ActiveGroupings.ObserveCollectionChanges()
            .Unit()
            .StartWith(Unit.Default)
            .Subscribe(_ => InactiveGroupings.LoadOptimized(AllGroupings.Except(ActiveGroupings)))
            .DisposeWith(this);

        AnalyzerVM.FilteredResults.ObserveCollectionChanges()
            .Subscribe(UpdateResultsTreeSource)
            .DisposeWith(this);

        UpdateResultsTreeSource();
    }

    [ReactiveCommand]
    private void AddGrouping(Grouping<AnalyzerResultVM> grouping)
    {
        if (!ActiveGroupings.Contains(grouping))
            ActiveGroupings.Add(grouping);
    }

    [ReactiveCommand]
    private void RemoveGrouping(Grouping<AnalyzerResultVM> grouping)
    {
        ActiveGroupings.Remove(grouping);
    }

    [ReactiveCommand]
    private void MoveGroupingUp(Grouping<AnalyzerResultVM> grouping)
    {
        var index = ActiveGroupings.IndexOf(grouping);
        if (index > 0)
            ActiveGroupings.Move(index, index - 1);
    }

    [ReactiveCommand]
    private void MoveGroupingDown(Grouping<AnalyzerResultVM> grouping)
    {
        var index = ActiveGroupings.IndexOf(grouping);
        if (index >= 0 && index < ActiveGroupings.Count - 1)
            ActiveGroupings.Move(index, index + 1);
    }

    [ReactiveCommand]
    private void ClearAllGrouping()
    {
        ActiveGroupings.Clear();
    }

    [ReactiveCommand]
    private void Update(SortableUpdateEventArgs args)
    {
        if (args.SourceCollection is IObservableCollection<Grouping<AnalyzerResultVM>> sourceCollection)
        {
            sourceCollection.Move(args.OldIndex, args.NewIndex);
        }
    }

    private void UpdateResultsTreeSource()
    {
        IEnumerable<object> items = ActiveGroupings.Count == 0
            ? FilteredResults
            : BuildHierarchy(FilteredResults).ToList();

        if (ResultsTreeSource is null)
        {
            ResultsTreeSource = new HierarchicalTreeDataGridSource<object>(items)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<object>(
                        new TemplateColumn<object>("Record", new FuncDataTemplate<object?>((item, _) => item switch
                        {
                            AnalyzerResultVM vm => GetTopicControl(vm),
                            GroupNode group => GetGroupNode(group),
                            _ => null
                        })),
                        o => o is GroupNode group ? group.Children : null
                    ),
                    new TemplateColumn<object>("Record Type", CreateTextCellTemplate(GetRecordTypeText)),
                    new TemplateColumn<object>("Topic", CreateTextCellTemplate(GetTopicText)),
                    new TemplateColumn<object>("Severity", CreateTextCellTemplate(GetSeverityControl)),
                    new TemplateColumn<object>("Parent Record", CreateTextCellTemplate(GetParentRecordText)),
                    new TemplateColumn<object>("Message", CreateTextCellTemplate(GetMessageText)),
                }
            };
        }
        else
        {
            ResultsTreeSource.Items = items;
        }
    }

    private IEnumerable<object> BuildHierarchy(IEnumerable<AnalyzerResultVM> items, int level = 0)
    {
        if (level >= ActiveGroupings.Count)
        {
            foreach (var item in items)
            {
                yield return item;
            }
            yield break;
        }

        var grouping = ActiveGroupings[level];
        var groups = items.GroupBy(item => grouping.Selector(item));

        foreach (var group in groups)
        {
            var groupNode = new GroupNode(group.Key ?? "(None)");
            var children = BuildHierarchy(group.ToList(), level + 1);

            foreach (var child in children)
            {
                groupNode.Children.Add(child);
            }

            yield return groupNode;
        }
    }

    private static TextBlock GetGroupNode(GroupNode group)
    {
        return group switch
        {
            _ => new TextBlock
            {
                Text = group.Key,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };
    }

    private static string? GetRecordTypeText(AnalyzerResultVM vm)
    {
        return RecordTypeConverters.GetName.Convert(vm.Result.Record?.Type)?.ToString();
    }

    private static string? GetTopicText(AnalyzerResultVM vm)
    {
        return vm.Result?.Topic?.TopicDefinition?.Title;
    }

    private static string? GetParentRecordText(AnalyzerResultVM vm)
    {
        return vm.RecordDisplayName;
    }

    private Control? GetMessageText(AnalyzerResultVM vm)
    {
        return _formattedTopicConverters.ExtractMessage.Convert(vm.Result.Topic.FormattedTopic) as Control;
    }

    private Control GetTopicControl(AnalyzerResultVM vm)
    {
        return new StackPanel
        {
            Spacing = 5,
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = null,
                    BorderBrush = null,
                    Padding = new Thickness(0, 0),
                    Content = new FASymbolIcon
                    {
                        Symbol = FASymbol.Open,
                        Foreground = StandardBrushes.HighlightBrush,
                    },
                    Command = OpenLinkCommands.OpenUriCommand,
                    CommandParameter = vm.Result.Topic.TopicDefinition.InformationUri,
                },
                new TextBlock
                {
                    Text = vm.Result.Topic.TopicDefinition.Title,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            }
        };
    }

    private Control GetSeverityControl(AnalyzerResultVM vm)
    {
        return new TextBlock
        {
            Text = vm.Result.Topic.Severity.ToString(),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = SeverityToBrushConverter.Convert(vm.Result.Topic.Severity),
        };
    }

    private static FuncDataTemplate<object?> CreateTextCellTemplate(Func<AnalyzerResultVM, Control?> valueSelector)
    {
        return new FuncDataTemplate<object?>((item, _) => item switch
        {
            AnalyzerResultVM vm => valueSelector(vm),
            _ => null
        });
    }

    private static FuncDataTemplate<object?> CreateTextCellTemplate(Func<AnalyzerResultVM, string?> valueSelector)
    {
        return new FuncDataTemplate<object?>((item, _) => item switch
        {
            AnalyzerResultVM vm => new TextBlock
            {
                Text = valueSelector(vm),
                VerticalAlignment = VerticalAlignment.Center,
            },
            _ => null
        });
    }
}
