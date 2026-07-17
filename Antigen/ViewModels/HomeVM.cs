using System.IO.Abstractions;
using System.Reactive.Subjects;
using Antigen.Views;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class HomeVM : ViewModel, IMainPanel
{
    private readonly IMainWindow _mainWindow;
    private readonly Subject<ModKey> _startRequested = new();

    public HomeVM(
        IMainWindow mainWindow,
        IFileSystem fileSystem,
        IDataDirectoryProvider dataDirectoryProvider,
        ILoadOrderListingsProvider loadOrderListingsProvider,
        ILogger<HomeVM> logger)
    {
        _mainWindow = mainWindow;
        Task.Run(() => LoadModKeys(fileSystem, dataDirectoryProvider, loadOrderListingsProvider))
            .FireAndForget(ex => logger.LogError(ex, "Error loading mod keys"));
    }

    public double CurrentWindowHeight => 40;
    public IObservable<ModKey> StartRequested => _startRequested;

    [Reactive] public partial ModKey? SelectedMod { get; set; } = null;
    [Reactive] public partial ModKey[] ModKeys { get; set; } = [];

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

    [ReactiveCommand]
    private void StartWatching()
    {
        if (!SelectedMod.HasValue || SelectedMod.Value.IsNull) return;

        _startRequested.OnNext(SelectedMod.Value);
    }

    [ReactiveCommand]
    private void Close()
    {
        _mainWindow.Close();
    }
}
