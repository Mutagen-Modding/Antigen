using Antigen.ViewModels;
using Avalonia.Controls;

namespace Antigen.Views.Analyzer;

public partial class DashboardView : UserControl
{
    public DashboardVM? ViewModel => DataContext as DashboardVM;

    public DashboardView()
    {
        InitializeComponent();
    }
}
