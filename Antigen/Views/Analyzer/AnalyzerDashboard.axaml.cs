using Antigen.ViewModels.Transient;
using ReactiveUI.Avalonia;

namespace Antigen.Views.Analyzer;

public partial class AnalyzerDashboard : ReactiveWindow<DashboardVM>
{
    public AnalyzerDashboard()
    {
        InitializeComponent();
    }

    public AnalyzerDashboard(DashboardVM vm) : this()
    {
        DataContext = ViewModel = vm;
    }
}
