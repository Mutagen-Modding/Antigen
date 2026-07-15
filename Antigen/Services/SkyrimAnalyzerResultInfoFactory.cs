using Antigen.Models.Analyzer;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Services;

public sealed class SkyrimAnalyzerResultInfoFactory : IAnalyzerResultInfoFactory
{
    /// <summary>
    ///     Creates an enriched analyzer result from raw analysis data.
    /// </summary>
    public AnalyzerResultInfo Create(AnalyzerResult result, ILinkCache linkCache)
    {
        string? resultEditorId = null;
        string? recordDisplayName;
        string? parentDisplayName = null;
        IMajorRecordIdentifierGetter? parentIdentifier = null;

        if (result.Record is not null)
        {
            resultEditorId = linkCache.TryResolveIdentifier(result.Record, out var resolvedEditorId) ? resolvedEditorId : null;

            if (resultEditorId is null)
            {
                // Special handling for exterior cells without editor ID
                // Call them "Worldspace - Wilderness (X, Y)" where X and Y are the grid coordinates and worldspace is the containing worldspace
                if (result.Record is ICellGetter cell
                 && !cell.Flags.HasFlag(Cell.Flag.IsInteriorCell)
                 && cell.Grid is { Point: var point }
                 && linkCache.TryResolveSimpleContext(cell, out var cellContext)
                 && cellContext.TryGetParent<IWorldspaceGetter>(out var worldspace))
                {
                    var worldspaceId = GetDisplayName(worldspace) ?? "UnknownWorldspace";
                    var gridX = point.X;
                    var gridY = point.Y;
                    recordDisplayName = $"{worldspaceId} - Wilderness ({gridX}, {gridY})";
                }
                else
                {
                    recordDisplayName = result.Record.FormKey.ToString();
                }
            }
            else
            {
                recordDisplayName = resultEditorId;
            }

            // Try to get the parent record's display name and identifier if it exists
            if (linkCache.TryResolveSimpleContext(result.Record, out var parentContext)
             && parentContext.Parent?.Record is IMajorRecordGetter parentRecord)
            {
                parentIdentifier = new MajorRecordIdentifier
                {
                    FormKey = parentRecord.FormKey,
                    EditorID = parentRecord.EditorID
                };
                parentDisplayName = GetDisplayName(parentRecord);
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

        string GetDisplayName(IMajorRecordGetter record)
        {
            return record.EditorID ?? record.FormKey.ToString();
        }
    }
}