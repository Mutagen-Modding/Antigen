using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;

/// <summary>
/// An analyzer targeting a single condition within a record
/// </summary>
public interface IConditionAnalyzer
{
    /// <summary>
    /// The topics this analyzer may report
    /// </summary>
    IEnumerable<TopicDefinition> Topics { get; }

    /// <summary>
    /// Callback to provide the condition data types this analyzer should be invoked for. <br />
    /// A condition is dispatched to this analyzer when its data is assignable to any of the returned types.
    /// </summary>
    /// <returns>List of condition data types of interest to the analyzer</returns>
    IEnumerable<Type> ConditionTypesOfInterest();

    /// <summary>
    /// Callback to execute the analyzer's logic
    /// </summary>
    /// <param name="context">Context for analysis</param>
    void AnalyzeCondition(ConditionAnalyzerContext context);
}
