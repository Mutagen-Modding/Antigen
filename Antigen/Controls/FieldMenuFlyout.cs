using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Antigen.Controls;

public sealed class FieldMenuFlyout : MenuFlyout
{
    private MenuFlyoutPresenter? _presenter;

    public FieldMenuFlyout()
    {
        Opened += (_, _) => Dispatcher.UIThread.Post(SelectFirstItem, DispatcherPriority.Loaded);
    }

    protected override Control CreatePresenter()
    {
        _presenter = (MenuFlyoutPresenter)base.CreatePresenter();
        _presenter.ContainerPrepared += (_, e) =>
        {
            if (e.Container is MenuItem item)
            {
                CenterSubMenu(item);
            }
        };

        return _presenter;
    }

    private void SelectFirstItem()
    {
        if (_presenter is null || HasSelection()) return;

        _presenter.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Source = _presenter,
            Key = Key.Down,
        });
    }

    private bool HasSelection() =>
        Enumerable.Range(0, _presenter!.ItemCount)
            .Select(_presenter.ContainerFromIndex)
            .Any(container => container is MenuItem { IsSelected: true });

    private static void CenterSubMenu(MenuItem item)
    {
        item.TemplateApplied += (_, e) => Center(e.NameScope.Find<Popup>("PART_Popup"));

        Center(item.GetVisualDescendants().OfType<Popup>().FirstOrDefault());

        static void Center(Popup? popup)
        {
            if (popup is not null)
            {
                popup.Placement = PlacementMode.Right;
            }
        }
    }
}
