using Antigen.ViewModels.Analyzer;
using Avalonia.Controls;

namespace Antigen.Views.Analyzer;

public partial class AnalyzerView : UserControl
{
    public AnalyzerView()
    {
        InitializeComponent();
    }

    public AnalyzerVM? ViewModel => DataContext as AnalyzerVM;
}
