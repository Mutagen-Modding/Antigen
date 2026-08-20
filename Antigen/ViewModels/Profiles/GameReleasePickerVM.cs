using System.Reactive.Linq;
using System.Windows.Input;
using Antigen.Services;
using Mutagen.Bethesda;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels.Profiles;

public sealed record GameChoice(string Name, IReadOnlyList<GameChoice> Releases, ICommand? Select);

public sealed partial class GameReleasePickerVM : ViewModel, ITransient
{
    public string Watermark { get; }

    public IReadOnlyList<GameChoice> Choices { get; }

    [Reactive] public partial GameRelease? Release { get; set; }

    [ObservableAsProperty(PropertyName = "Display", InitialValue = "string.Empty")]
    private IObservable<string> DisplayObservable() =>
        this.WhenAnyValue(x => x.Release)
            .Select(release => release?.ToDescriptionString() ?? Watermark);

    public GameReleasePickerVM(string watermark)
    {
        Watermark = watermark;

        InitializeOAPH();

        Choices = GameSupport.Categories.Select(Choice).ToArray();
    }

    private GameChoice Choice(GameCategory category)
    {
        var releases = GameSupport.ReleasesFor(category);

        // A category with a single release has nothing to choose, so don't open a submenu for it.
        return releases is [var only]
            ? new GameChoice(category.ToDescriptionString(), [], Choose(only))
            : new GameChoice(category.ToDescriptionString(), releases.Select(Leaf).ToArray(), null);
    }

    private GameChoice Leaf(GameRelease release) => new(release.ToDescriptionString(), [], Choose(release));

    private ICommand Choose(GameRelease release) =>
        ReactiveCommand.Create(() => Release = release).DisposeWith(this);
}
