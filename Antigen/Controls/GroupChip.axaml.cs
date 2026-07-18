using System.Windows.Input;
using Antigen.Views.Analyzer;
using Avalonia;
using Avalonia.Controls;

namespace Antigen.Controls;

public partial class GroupChip : UserControl
{
    public static readonly StyledProperty<ICommand> RemoveCommandProperty = AvaloniaProperty.Register<AnalyzerDashboard, ICommand>(nameof(RemoveCommand));
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<AnalyzerDashboard, string>(nameof(Label));

    public ICommand RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public GroupChip()
    {
        InitializeComponent();
    }
}
