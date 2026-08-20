using Antigen.Modules;
using Mutagen.Bethesda;

namespace Antigen.Services;

public static class GameSupport
{
    private static readonly Dictionary<GameCategory, Func<GameCategoryModule>> Modules = new()
    {
        [GameCategory.Skyrim] = () => new SkyrimModule(),
    };

    private static readonly Dictionary<GameCategory, GameRelease[]> ReleasesByCategory =
        Modules.Keys.ToDictionary(category => category, category => category.GetRelatedReleases().ToArray());

    public static GameCategory[] Categories { get; } = [..Modules.Keys];

    public static GameRelease[] ReleasesFor(GameCategory category) =>
        ReleasesByCategory.GetValueOrDefault(category, []);

    public static bool IsSupported(GameRelease release) => Modules.ContainsKey(release.ToCategory());

    public static GameCategoryModule ModuleFor(GameRelease release) => Modules[release.ToCategory()]();
}
