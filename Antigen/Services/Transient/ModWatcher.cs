using System.IO.Abstractions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Antigen.Models.Analyzer;
using Antigen.Services.Singleton;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace Antigen.Services.Transient;

public interface IModWatcher : IDisposable
{
    IObservable<IObservable<AnalyzerResultInfo>> AnalysisCompleted { get; }
    IObservable<StatusUpdate> StatusUpdates { get; }
    Severity MinimumSeverity { get; set; }
    void Start();
    void Stop();
}

public sealed class ModWatcher(
    IFileSystem fileSystem,
    IDataDirectoryProvider dataDirectoryProvider,
    ModKey modKey,
    IAnalyzerService analyzerService,
    ILogger<ModWatcher> logger)
    : IModWatcher
{
    private readonly DisposableBucket _disposables = new();
    private readonly Subject<IObservable<AnalyzerResultInfo>> _analysisCompleted = new();
    private readonly string _filePath = fileSystem.Path.Combine(dataDirectoryProvider.Path, modKey.FileName);
    private readonly IFileSystemWatcher _fileSystemWatcher = fileSystem.FileSystemWatcher.New(dataDirectoryProvider.Path, modKey.FileName);
    private CancellationTokenSource _cancellationTokenSource = new();
    private DateTime _lastWriteTime = DateTime.Now;
    private IDisposable? _subscription;
    public IObservable<IObservable<AnalyzerResultInfo>> AnalysisCompleted => _analysisCompleted;

    public IObservable<StatusUpdate> StatusUpdates => analyzerService.StatusUpdates;

    public Severity MinimumSeverity
    {
        get => analyzerService.MinimumSeverity;
        set => analyzerService.MinimumSeverity = value;
    }

    public void Start()
    {
        // To handle scenarios like MO2 where the change
        Observable.Interval(TimeSpan.FromSeconds(2))
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Subscribe(t =>
            {
                var currentLastWriteTime = fileSystem.File.GetLastWriteTime(_filePath);
                if (currentLastWriteTime <= _lastWriteTime) return;

                _lastWriteTime = currentLastWriteTime;
                _ = OnFileChanged();
            })
            .DisposeWith(_disposables);

        _subscription = ObservableExtensions.Subscribe(_fileSystemWatcher.Events()
                .Changed
                .Throttle(TimeSpan.FromMilliseconds(500), RxSchedulers.TaskpoolScheduler)
                .ObserveOn(RxSchedulers.TaskpoolScheduler), x => _ = OnFileChanged())
            .DisposeWith(_disposables);

        _fileSystemWatcher.EnableRaisingEvents = true;

        logger.LogInformation("Started watching {ModKey} at {FilePath}", modKey, _filePath);

        RxSchedulers.TaskpoolScheduler.Schedule(async void () =>
        {
            try
            {
                await OnFileChanged();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during initial analysis");
            }
        });
    }

    public void Stop()
    {
        _fileSystemWatcher.EnableRaisingEvents = false;
        _subscription?.Dispose();
        _cancellationTokenSource.Cancel();

        logger.LogInformation("Stopped watching {ModKey}", modKey);
    }

    public void Dispose()
    {
        Stop();
        _fileSystemWatcher.Dispose();
        _disposables.Dispose();
    }

    private async Task OnFileChanged()
    {
        logger.LogInformation("Change detected for {ModKey}.  Restarting analysis", modKey);

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        var results = analyzerService.AnalyzeAsync(modKey, _cancellationTokenSource.Token)
            .ToObservable()
            .Publish()
            .RefCount();

        _analysisCompleted.OnNext(results);
    }
}
