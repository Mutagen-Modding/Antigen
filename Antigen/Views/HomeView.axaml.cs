using Antigen.ViewModels.Singleton;
using Avalonia.Controls;

namespace Antigen.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    public HomeVM? ViewModel => DataContext as HomeVM;
}
