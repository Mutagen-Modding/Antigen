namespace Antigen.ViewModels;

public interface IResizablePanel : IMainPanel
{
    bool IsExpanded { get; }
    double ExpandedHeight { get; set; }
    double MinResizeHeight { get; }
    double MaxResizeHeight { get; }
    void Resize(double height);
}
