using System.Reactive.Linq;
using Antigen.Models.Settings;
using Antigen.Services;
using Noggog;
using Noggog.WorkEngine;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class GlobalSettingsVM : ResizablePanelVM, INumWorkThreadsController, ISingleton
{
    public const double DefaultPercentage = 0.5;

    public override double MinResizeHeight => 150.0;

    [Reactive] public partial double CorePercentage { get; set; }

    [Reactive] public partial ColorScheme ColorScheme { get; set; }

    public static ColorScheme[] ColorSchemes { get; } = Enum.GetValues<ColorScheme>();

    [ObservableAsProperty]
    private IObservable<int> WorkerThreads() =>
        this.WhenAnyValue(x => x.CorePercentage).Select(ToThreadCount);

    public IObservable<int?> NumDesiredThreads =>
        this.WhenAnyValue(x => x.CorePercentage).Select(p => (int?)ToThreadCount(p));

    private readonly ActiveVmController _activeVm;
    private readonly HomeVM _homeVM;

    public GlobalSettingsVM(
        ActiveVmController activeVm,
        HomeVM homeVM,
        GuiSettingsService guiSettings,
        ColorSchemeService colorSchemes)
    {
        _activeVm = activeVm;
        _homeVM = homeVM;
        IsExpanded = true;

        var saved = guiSettings.Load();
        CorePercentage = Math.Clamp(saved?.WorkerThreadPercentage ?? DefaultPercentage, 0, 1);
        ColorScheme = saved?.ColorScheme ?? ColorSchemeService.Default;

        InitializeOAPH();

        this.WhenAnyValue(x => x.ColorScheme)
            .Subscribe(colorSchemes.Apply)
            .DisposeWith(this);
    }

    [ReactiveCommand]
    private void Back()
    {
        _activeVm.Active = _homeVM;
    }

    private static int ToThreadCount(double percentage) =>
        Math.Max(1, (int)(Environment.ProcessorCount * Math.Clamp(percentage, 0, 1)));
}
