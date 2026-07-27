using System.Reactive.Linq;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public abstract partial class ResizablePanelVM : ViewModel, IResizablePanel
{
    protected const double CollapsedHeight = 40.0;

    [Reactive] public partial bool IsExpanded { get; set; }
    [Reactive] public partial double ExpandedHeight { get; set; } = 500.0;
    [Reactive] public partial double CurrentWindowHeight { get; set; } = CollapsedHeight;

    public virtual double MinResizeHeight => 200.0;
    public virtual double MaxResizeHeight => 1000.0;

    protected ResizablePanelVM()
    {
        this.WhenAnyValue(x => x.IsExpanded, x => x.ExpandedHeight)
            .Subscribe(_ => CurrentWindowHeight = IsExpanded ? ExpandedHeight : CollapsedHeight)
            .DisposeWith(this);
    }

    public void Resize(double height)
    {
        ExpandedHeight = Math.Clamp(height, MinResizeHeight, MaxResizeHeight);
        IsExpanded = true;
    }

    [ReactiveCommand]
    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }
}
