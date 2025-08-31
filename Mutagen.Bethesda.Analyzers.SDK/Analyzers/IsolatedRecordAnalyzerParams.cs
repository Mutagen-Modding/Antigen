using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.SDK.Analyzers;

/// <summary>
/// Object containing all the parameters available for a <see cref="IIsolatedRecordAnalyzer{TMajor}"/>
/// </summary>
/// <typeparam name="TMajor">The type of record being analyzed</typeparam>
public readonly record struct IsolatedRecordAnalyzerParams<TMajor>
    where TMajor : IMajorRecordGetter
{
    public Type? AnalyzerType { get; init; }
    private readonly ModKey _mod;
    private readonly ReportContextParameters _parameters;
    private readonly IReportDropbox _reportDropbox;

    /// <summary>
    /// The record to be analyzed
    /// </summary>
    public readonly TMajor Record;

    public IsolatedRecordAnalyzerParams(
        ModKey mod,
        TMajor record,
        ReportContextParameters parameters,
        IReportDropbox reportDropbox)
    {
        _mod = mod;
        Record = record;
        _parameters = parameters;
        _reportDropbox = reportDropbox;
    }

    /// <summary>
    /// Reports a topic to the engine
    /// </summary>
    public void AddTopic(
        IFormattedTopicDefinition formattedTopicDefinition,
        params (string Name, object Value)[] metaData)
    {
        _reportDropbox.Dropoff(
            _parameters,
            _mod,
            Record,
            Topic.Create(formattedTopicDefinition, AnalyzerType, metaData));
    }
}
