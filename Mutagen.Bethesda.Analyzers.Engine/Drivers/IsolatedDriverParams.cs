using System.IO.Abstractions;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Drivers;

public class IsolatedDriverParams
{
    public readonly ILinkCache LinkCache;
    public readonly IReportDropbox ReportDropbox;
    public readonly IModGetter TargetMod;
    public readonly IsolatedDriverFileParams? FileParams;
    public readonly CancellationToken CancellationToken;


    public IsolatedDriverParams(
        ILinkCache linkCache,
        IReportDropbox reportDropbox,
        IModGetter targetMod,
        IsolatedDriverFileParams? fileParams,
        CancellationToken cancellationToken)
    {
        LinkCache = linkCache;
        ReportDropbox = reportDropbox;
        TargetMod = targetMod;
        FileParams = fileParams;
        CancellationToken = cancellationToken;
    }
}

public class IsolatedDriverFileParams(
    IFileSystem fileSystem,
    ModPath modPath)
{
    public IFileSystem FileSystem { get; } = fileSystem;
    public ModPath ModPath { get; } = modPath;
}
