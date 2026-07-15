using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Antigen.Views;

public partial class MainWindow : PinnedWindow
{

    public MainWindow()
    {
        InitializeComponent();

        MainBar = Bar;
    }
    public override sealed Control MainBar { get; set; }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}