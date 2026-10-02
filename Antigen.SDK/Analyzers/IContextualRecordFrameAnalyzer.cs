using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.SDK.Analyzers;

public interface IContextualRecordFrameAnalyzer<TMajor> : IAnalyzer
    where TMajor : IMajorRecordGetter
{
    void AnalyzeRecord(ContextualRecordFrameAnalyzerParams<TMajor> param);
}

public interface IContextualRecordFrameAnalyzer : IAnalyzer
{
    bool AcceptsType<TMajor>() where TMajor : IMajorRecordGetter;

    void AnalyzeRecord(ContextualRecordFrameAnalyzerParams param);
}
