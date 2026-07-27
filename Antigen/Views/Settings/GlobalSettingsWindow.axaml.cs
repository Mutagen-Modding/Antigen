using Antigen.ViewModels.Singleton;
using Antigen.Views;
using Avalonia.Interactivity;

namespace Antigen.Views.Settings;

public partial class GlobalSettingsWindow : PinnedWindow
{
    public GlobalSettingsWindow()
    {
        InitializeComponent();
    }

    public GlobalSettingsWindow(GlobalSettingsVM viewModel)
    {
        DataContext = viewModel;

        InitializeComponent();
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
