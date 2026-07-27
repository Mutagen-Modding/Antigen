using Antigen.ViewModels.Transient;
using Avalonia.Controls;

namespace Antigen.Views.Analyzer;

public partial class AnalyzerView : UserControl
{
    public AnalyzerVM? ViewModel => DataContext as AnalyzerVM;

    public AnalyzerView()
    {
        InitializeComponent();
    }
}
