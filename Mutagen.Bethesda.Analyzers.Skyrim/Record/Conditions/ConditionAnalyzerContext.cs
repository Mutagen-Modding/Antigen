using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;

/// <summary>
/// Object containing all the parameters available for a <see cref="IConditionAnalyzer"/>
/// </summary>
public readonly struct ConditionAnalyzerContext
{
    /// <summary>
    /// Record analyzer parameters for the condition's owning record
    /// </summary>
    public readonly ContextualRecordAnalyzerParams<ISkyrimMajorRecordGetter> Param;

    /// <summary>
    /// All conditions in the current field block
    /// </summary>
    public readonly IReadOnlyList<IConditionGetter> Conditions;

    /// <summary>
    /// Index of the current condition within <see cref="Conditions"/>
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// The OR block that the current condition belongs to
    /// </summary>
    public readonly IReadOnlyList<IConditionGetter> OrBlock;

    /// <summary>
    /// The condition currently being analyzed
    /// </summary>
    public IConditionGetter Condition => Conditions[Index];

    public ConditionAnalyzerContext(
        ContextualRecordAnalyzerParams<ISkyrimMajorRecordGetter> param,
        IReadOnlyList<IConditionGetter> conditions,
        int index,
        IReadOnlyList<IConditionGetter> orBlock)
    {
        Param = param;
        Conditions = conditions;
        Index = index;
        OrBlock = orBlock;
    }
}
