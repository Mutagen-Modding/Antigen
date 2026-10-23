using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Extensions;

public static class ConditionExtensions
{
    /// <summary>
    /// Split a condition list into OR blocks, where at least one condition from each block must pass
    /// </summary>
    /// <param name="conditions"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<IConditionGetter>> SplitOrBlocks(this IEnumerable<IConditionGetter> conditions)
    {
        List<IConditionGetter> block = [];
        foreach (var condition in conditions)
        {
            block.Add(condition);
            if (!condition.Flags.HasFlag(Condition.Flag.OR))
            {
                yield return block;
                block = [];
            }
        }
        if (block.Count > 0)
            yield return block;
    }

    public static float? GetInitialValue(this IConditionGlobalGetter condition, ILinkCache linkCache)
    {
        if (condition.ComparisonValue.TryResolve(linkCache, out var global))
        {
            return global switch
            {
                IGlobalFloatGetter globalFloatGetter => globalFloatGetter.Data,
                IGlobalIntGetter globalIntGetter => globalIntGetter.Data,
                IGlobalShortGetter globalShortGetter => globalShortGetter.Data,
                _ => throw new InvalidOperationException($"Unexpected global type {global.GetType().Name}")
            };
        }

        return null;
    }
}
