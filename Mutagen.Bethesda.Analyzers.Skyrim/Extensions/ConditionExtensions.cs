using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

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
}
