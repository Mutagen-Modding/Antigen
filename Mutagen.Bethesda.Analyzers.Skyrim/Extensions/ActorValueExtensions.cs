using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ActorValueExtensions
{
    /// <summary>
    /// Returns true if the actor value is a skill actor value like OneHanded, Enchanting, etc. that can be learned.
    /// </summary>
    /// <param name="actorValue">Actor value to check</param>
    /// <returns>True if the actor value is a skill actor value</returns>
    public static bool IsSkill(this ActorValue actorValue)
    {
        return (int)actorValue is >= 6 and <= 23;
    }
}
