using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util;

public class MissingAssetsAnalyzerUtil
{
    private readonly IFileSystem _fileSystem;
    private readonly IDataDirectoryProvider _dataDirectory;

    public MissingAssetsAnalyzerUtil(IFileSystem fileSystem, IDataDirectoryProvider dataDirectory)
    {
        _fileSystem = fileSystem;
        _dataDirectory = dataDirectory;
    }

    public void CheckForMissingModelAsset<TMajorRecordGetter>(
        IsolatedRecordAnalyzerParams<TMajorRecordGetter> param,
        TopicDefinition<string> topicDefinition)
        where TMajorRecordGetter : IMajorRecordGetter, IModeledGetter
    {
        var path = param.Record.Model?.File;
        if (path is null) return;
        if (FileExists(path)) return;

        param.AddTopic(topicDefinition.Format(path));
    }

    public bool FileExists(IAssetLinkGetter path) => _fileSystem.File.Exists(Path.Join(_dataDirectory.Path, path.DataRelativePath.Path));
    public bool FileExistsIfNotNull([NotNullWhen(false)] IAssetLinkGetter? path) => path == null || FileExists(path);
}
