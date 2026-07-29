using Antigen.ViewModels;
using Avalonia.Controls;

namespace Antigen.Views;

public partial class HomeView : UserControl
{
    public HomeVM? ViewModel => DataContext as HomeVM;

    public HomeView()
    {
        InitializeComponent();
    }
}
