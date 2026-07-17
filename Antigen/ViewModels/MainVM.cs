using System.IO.Abstractions;
using Antigen.ViewModels.Analyzer;
using Antigen.Views.Analyzer;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class MainVM : ViewModel
{

    private readonly Func<ModKey, ModWatcherVM> _modWatcherVMFactory;
    private readonly Func<ModWatcherVM, AnalyzerVM> _pinnedModeVMFactory;

    public MainVM(
        Func<ModKey, ModWatcherVM> modWatcherVMFactory,
        Func<ModWatcherVM, AnalyzerVM> pinnedModeVMFactory,
        IFileSystem fileSystem,
        IDataDirectoryProvider dataDirectoryProvider,
        ILoadOrderListingsProvider loadOrderListingsProvider,
        ILogger<MainVM> logger)
    {
        _modWatcherVMFactory = modWatcherVMFactory;
        _pinnedModeVMFactory = pinnedModeVMFactory;
        Task.Run(() => LoadModKeys(fileSystem, dataDirectoryProvider, loadOrderListingsProvider))
            .FireAndForget(ex => logger.LogError(ex, "Error loading mod keys"));
    }
    public static Severity[] SeverityValues { get; } = Enum.GetValues<Severity>();

    public ModWatcherVM? CurrentWatcher { get; set; }
    [Reactive] public partial ModKey? SelectedMod { get; set; } = null;
    [Reactive] public partial AnalyzerVM? AnalyzerVM { get; set; } = null;

    [Reactive] public partial ModKey[] ModKeys { get; set; } = [];
    [Reactive] public partial string SelectedModFileName { get; set; } = string.Empty;

    private void LoadModKeys(
        IFileSystem fileSystem,
        IDataDirectoryProvider dataDirectoryProvider,
        ILoadOrderListingsProvider loadOrderListingsProvider)
    {
        ModKeys = loadOrderListingsProvider.Get()
            .Where(l => fileSystem.File.Exists(fileSystem.Path.Combine(dataDirectoryProvider.Path, l.FileName)))
            .Select(l => ModKey.FromFileName(l.FileName))
            .ToArray();
    }

    [ReactiveCommand(OutputScheduler = "TaskpoolScheduler")]
    private void StartWatching()
    {
        if (App.MainAppWindow is null) return;
        if (!SelectedMod.HasValue || SelectedMod.Value.IsNull) return;

        CurrentWatcher?.Dispose();
        CurrentWatcher = _modWatcherVMFactory(SelectedMod.Value);

        AnalyzerVM = _pinnedModeVMFactory(CurrentWatcher);

        var modAnalyzerWindow = new AnalyzerWindow(AnalyzerVM)
        {
            Position = App.MainAppWindow.Position
        };
        modAnalyzerWindow.Show();

        // Hide main window for now
        App.MainAppWindow.Hide();

        // Re-open main window after analyzer window is closed
        AnalyzerVM.ReturnRequested
            .Subscribe(_ =>
            {
                App.MainAppWindow.Position = modAnalyzerWindow.Position;
                App.MainAppWindow.Show();
                modAnalyzerWindow.Close();
            })
            .DisposeWith(this);
    }
}
