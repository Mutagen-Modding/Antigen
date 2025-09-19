using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.SDK.Analyzers;

public readonly struct ContextualRecordFrameAnalyzerParams<TMajor>
    where TMajor : IMajorRecordGetter
{
    public readonly ILinkCache LinkCache;
    public readonly MajorRecordFrame Frame;

    public ContextualRecordFrameAnalyzerParams(ILinkCache linkCache, MajorRecordFrame frame)
    {
        LinkCache = linkCache;
        Frame = frame;
    }
}

public readonly struct ContextualRecordFrameAnalyzerParams
{
    public readonly ILinkCache LinkCache;
    public readonly MajorRecordFrame Frame;

    public ContextualRecordFrameAnalyzerParams(ILinkCache linkCache, MajorRecordFrame frame)
    {
        LinkCache = linkCache;
        Frame = frame;
    }
}
