using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace Antigen.Resources.Behavior;

// Adapted from https://github.com/AvaloniaUI/Avalonia/discussions/13301
public sealed class AutoCompleteZeroMinimumPrefixLengthDropdownBehaviour : Behavior<AutoCompleteBox>
{
    protected override void OnAttached()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.KeyUp += OnKeyUp;
            AssociatedObject.DropDownOpening += DropDownOpening;
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.PointerReleased += PointerReleased;

            Task.Delay(500).ContinueWith(_ => Dispatcher.UIThread.Invoke(() => CreateDropdownButton(AssociatedObject)));
        }

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.KeyUp -= OnKeyUp;
            AssociatedObject.DropDownOpening -= DropDownOpening;
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.PointerReleased -= PointerReleased;
        }

        base.OnDetaching();
    }

    //have to use KeyUp as AutoCompleteBox eats some of the KeyDown events
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (e.Key is not (Key.Down or Key.F4)) return;
        if (!string.IsNullOrEmpty(AssociatedObject.Text)) return;

        ShowDropdown(AssociatedObject);
    }

    private void DropDownOpening(object? sender, CancelEventArgs e)
    {
        var prop = AssociatedObject?.GetType().GetProperty("TextBox", BindingFlags.Instance | BindingFlags.NonPublic);
        var tb = (TextBox?)prop?.GetValue(AssociatedObject);
        if (tb is null || !tb.IsReadOnly) return;

        e.Cancel = true;
    }

    private void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (!string.IsNullOrEmpty(AssociatedObject.Text)) return;

        ShowDropdown(AssociatedObject);
    }

    private void OnGotFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject is null) return;

        CreateDropdownButton(AssociatedObject);
    }

    private static void ShowDropdown(AutoCompleteBox associatedObject)
    {
        if (associatedObject.IsDropDownOpen) return;

        typeof(AutoCompleteBox).GetMethod("PopulateDropDown", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(associatedObject, [associatedObject, EventArgs.Empty]);
        typeof(AutoCompleteBox).GetMethod("OpeningDropDown", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(associatedObject, [false]);

        if (associatedObject.IsDropDownOpen) return;

        // We *must* set the field and not the property as we need to avoid the changed event being raised (which prevents the dropdown opening).
        var ipc = typeof(AutoCompleteBox).GetField("_ignorePropertyChange", BindingFlags.NonPublic | BindingFlags.Instance);
        if (ipc?.GetValue(associatedObject) is not bool b) return;

        if (!b)
        {
            ipc.SetValue(associatedObject, true);
        }

        associatedObject.SetCurrentValue(AutoCompleteBox.IsDropDownOpenProperty, true);
    }

    private static void CreateDropdownButton(AutoCompleteBox associatedObject)
    {
        var prop = associatedObject.GetType().GetProperty("TextBox", BindingFlags.Instance | BindingFlags.NonPublic);
        if (prop?.GetValue(associatedObject) is not TextBox tb || tb.InnerRightContent is Button) return;

        var btn = new Button
        {
            Content = "⯆",
            Margin = new Thickness(3),
            Padding = new Thickness(6, 2),
            ClickMode = ClickMode.Press
        };
        btn.Click += (_, _) =>
        {
            associatedObject.Text = string.Empty;
            ShowDropdown(associatedObject);
        };

        tb.InnerRightContent = btn;
    }
}