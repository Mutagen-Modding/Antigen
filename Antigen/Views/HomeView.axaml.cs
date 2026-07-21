using Antigen.ViewModels.Singleton;
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
