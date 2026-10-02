using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.SDK.Analyzers;

/// <summary>
/// An analyzer targeting a specific record's content only
/// </summary>
/// <typeparam name="TMajor">The type of record being analyzed</typeparam>
public interface IIsolatedRecordAnalyzer<TMajor> : IAnalyzer
    where TMajor : IMajorRecordGetter
{
    /// <summary>
    /// Callback to execute the analyzer's logic
    /// </summary>
    /// <param name="param">Parameters for analysis</param>
    void AnalyzeRecord(IsolatedRecordAnalyzerParams<TMajor> param);

    /// <summary>
    /// Callback to provide the fields that relate to an analyzer's analysis. <br />
    /// This is used to inform the engine of when the analyzer should be rerun.
    /// </summary>
    /// <returns>List of fields of interest to the analyzer</returns>
    IEnumerable<Func<TMajor, object?>> FieldsOfInterest();
}
