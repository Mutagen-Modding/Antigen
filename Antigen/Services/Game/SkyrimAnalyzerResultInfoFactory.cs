using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Services.Game;

public sealed class SkyrimAnalyzerResultInfoFactory : AnalyzerResultInfoFactory
{
    protected override string? Describe(IFormLinkIdentifier record, ILinkCache linkCache)
    {
        if (record is ICellGetter { Grid.Point: var point } cell
         && !cell.Flags.HasFlag(Cell.Flag.IsInteriorCell)
         && linkCache.TryResolveSimpleContext(cell, out var cellContext)
         && cellContext.TryGetParent<IWorldspaceGetter>(out var worldspace))
        {
            return $"{DisplayName(worldspace)} - Wilderness ({point.X}, {point.Y})";
        }

        return null;
    }
}
