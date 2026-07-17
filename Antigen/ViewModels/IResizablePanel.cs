namespace Antigen.ViewModels;

public interface IResizablePanel : IMainPanel
{
    bool IsExpanded { get; }
    double MinResizeHeight { get; }
    double MaxResizeHeight { get; }
    void Resize(double height);
}
