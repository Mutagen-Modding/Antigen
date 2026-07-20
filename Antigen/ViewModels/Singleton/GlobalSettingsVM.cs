using System.Reactive.Linq;
using Antigen.Services.Singleton;
using Noggog.WorkEngine;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels.Singleton;

public sealed partial class GlobalSettingsVM : ViewModel, INumWorkThreadsController
{
    public const double DefaultPercentage = 0.5;

    [Reactive] public partial double CorePercentage { get; set; }

    [ObservableAsProperty]
    private IObservable<int> WorkerThreads() =>
        this.WhenAnyValue(x => x.CorePercentage).Select(ToThreadCount);

    public IObservable<int?> NumDesiredThreads =>
        this.WhenAnyValue(x => x.CorePercentage).Select(p => (int?)ToThreadCount(p));

    public GlobalSettingsVM(GuiSettingsService guiSettings)
    {
        var saved = guiSettings.Load()?.WorkerThreadPercentage ?? DefaultPercentage;
        CorePercentage = Math.Clamp(saved, 0, 1);

        InitializeOAPH();
    }

    private static int ToThreadCount(double percentage) =>
        Math.Max(1, (int)(Environment.ProcessorCount * Math.Clamp(percentage, 0, 1)));
}
