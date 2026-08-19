using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class NavigationController : ReactiveObject, ISingleton
{
    private readonly HomeVM _home;

    private readonly List<ResizablePanelVM> _history = [];

    [Reactive] public partial ResizablePanelVM? Active { get; private set; }

    public NavigationController(HomeVM home)
    {
        _home = home;
        Active = home;
    }

    public void GoTo(ResizablePanelVM panel)
    {
        _history.Clear();
        Active = panel;
    }

    public void Push(ResizablePanelVM panel)
    {
        if (Active == panel) return;

        if (Active is { } leaving)
        {
            _history.Add(leaving);
        }
        Active = panel;
    }

    public void Back()
    {
        if (_history.Count == 0)
        {
            Active = _home;
            return;
        }

        var index = _history.Count - 1;
        Active = _history[index];
        _history.RemoveAt(index);
    }
}
