using Antigen.ViewModels.Transient;
using Avalonia.Controls;

namespace Antigen.Views.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsVM settingsVM)
    {
        DataContext = settingsVM;

        InitializeComponent();
    }
}