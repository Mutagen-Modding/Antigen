using System.Reactive.Linq;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public abstract partial class ResizablePanelVM : ViewModel
{
    protected const double CollapsedHeight = 40.0;
    protected const double PeekHeight = 260.0;
    protected const double DefaultPanelWidth = 1050.0;
    protected const double CapsuleWidth = 800.0;

    [Reactive] public partial bool IsExpanded { get; set; }
    [Reactive] public partial bool IsPeeking { get; set; }
    [Reactive] public partial double ExpandedHeight { get; set; } = 500.0;
    [Reactive] public partial double ExpandedWidth { get; set; } = DefaultPanelWidth;
    [Reactive] public partial double CurrentWindowHeight { get; set; } = CollapsedHeight;
    [Reactive] public partial double CurrentWindowWidth { get; set; } = CapsuleWidth;

    public virtual double MinResizeHeight => 200.0;
    public virtual double MaxResizeHeight => 1000.0;
    public virtual double MinResizeWidth => 700.0;
    public virtual double MaxResizeWidth => 2400.0;

    protected ResizablePanelVM()
    {
        this.WhenAnyValue(x => x.IsExpanded, x => x.IsPeeking, x => x.ExpandedHeight, x => x.ExpandedWidth)
            .Subscribe(_ =>
            {
                CurrentWindowHeight = IsExpanded
                    ? ExpandedHeight
                    : IsPeeking
                        ? PeekHeight
                        : CollapsedHeight;
                CurrentWindowWidth = IsExpanded ? ExpandedWidth : CapsuleWidth;
            })
            .DisposeWith(this);
    }

    public void Resize(double width, double height)
    {
        ExpandedWidth = Math.Clamp(width, MinResizeWidth, MaxResizeWidth);
        ExpandedHeight = Math.Clamp(height, MinResizeHeight, MaxResizeHeight);
        IsExpanded = true;
    }
}
