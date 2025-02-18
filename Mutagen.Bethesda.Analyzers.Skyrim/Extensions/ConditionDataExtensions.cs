using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ConditionDataExtensions
{
    public static bool RunsOnPlayer(this IConditionDataGetter condition)
    {
        return condition.RunOnType == Condition.RunOnType.Reference
               && condition.Reference.FormKey == FormKeys.SkyrimSE.Skyrim.PlayerRef.FormKey;
    }
}
