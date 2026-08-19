using Antigen.Models.Analyzer;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Services.Game;

public class AnalyzerResultInfoFactory : IAnalyzerResultInfoFactory
{
    public AnalyzerResultInfo Create(AnalyzerResult result, ILinkCache linkCache)
    {
        string? resultEditorId = null;
        string? recordDisplayName;
        string? parentDisplayName = null;
        IMajorRecordIdentifierGetter? parentIdentifier = null;

        if (result.Record is not null)
        {
            resultEditorId = linkCache.TryResolveIdentifier(result.Record, out var resolvedEditorId) ? resolvedEditorId : null;

            recordDisplayName = resultEditorId
                             ?? Describe(result.Record, linkCache)
                             ?? result.Record.FormKey.ToString();

            if (linkCache.TryResolveSimpleContext(result.Record, out var parentContext)
             && parentContext.Parent?.Record is IMajorRecordGetter parentRecord)
            {
                parentIdentifier = new MajorRecordIdentifier
                {
                    FormKey = parentRecord.FormKey,
                    EditorID = parentRecord.EditorID
                };
                parentDisplayName = DisplayName(parentRecord);
            }
        }
        else
        {
            recordDisplayName = "Unknown Record";
        }

        return new AnalyzerResultInfo
        {
            Result = result,
            ResultEditorId = resultEditorId,
            RecordDisplayName = recordDisplayName,
            ParentDisplayName = parentDisplayName,
            ParentIdentifier = parentIdentifier
        };
    }

    protected virtual string? Describe(IFormLinkIdentifier record, ILinkCache linkCache) => null;

    protected static string DisplayName(IMajorRecordGetter record) => record.EditorID ?? record.FormKey.ToString();
}
