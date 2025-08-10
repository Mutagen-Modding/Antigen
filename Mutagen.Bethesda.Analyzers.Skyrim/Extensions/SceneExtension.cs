using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class SceneExtension
{
    public static ISceneActionGetter? GetAction(
        this ISceneGetter scene,
        int sceneActionIndex)
    {
        return scene.Actions.FirstOrDefault(a => a.Index == sceneActionIndex);
    }
}
