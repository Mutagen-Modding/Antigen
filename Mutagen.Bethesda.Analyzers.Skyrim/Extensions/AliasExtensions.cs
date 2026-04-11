using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class AliasExtensions
{
    public static bool IsForcedNone(this IQuestAliasGetter alias, IQuestGetter quest)
    {
        if (!alias.ForcedReference.IsNull
                    || !alias.UniqueActor.IsNull
                    || alias.Location != null
                    || alias.External != null
                    || alias.CreateReferenceToObject != null
                    || alias.FindMatchingRefFromEvent != null
                    || alias.FindMatchingRefNearAlias != null
                    // An alias with "find matching ref" and no conditions is
                    // encoded identically to forced none
                    // Ditto for "find matching ref in loaded area"
                    || alias.Conditions.Any()
                    || !alias.SpecificLocation.IsNull)
        {
            return false;
        }
        return !quest.Aliases.Any(other => other.AliasIDToForceIntoWhenFilled == alias.ID);
    }
}
