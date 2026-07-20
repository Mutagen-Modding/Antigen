using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using System.Text.Json;
using Antigen.Models.Analyzer;
using Antigen.Models.Settings;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Plugins;

namespace Antigen.Services.Singleton;

public interface ISettingsService
{
    IObservable<ImmutableArray<IgnoreRule>> RulesChanged { get; }
    ImmutableArray<IgnoreRule> GetRules(ModKey modKey);
    void AddRule(ModKey modKey, IgnoreRule rule);
    void AddRule(ModKey modKey, AnalyzerResultInfo resultInfo, IgnoreType ignoreType);
    void RemoveRule(ModKey modKey, int index);
    void ClearRules(ModKey modKey);
    bool IsIgnored(ModKey modKey, AnalyzerResultInfo resultInfo);
}

public sealed class SettingsService : ISettingsService
{
    private readonly Dictionary<ModKey, List<IgnoreRule>> _cache = new();
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<SettingsService> _logger;

    private readonly Subject<ImmutableArray<IgnoreRule>> _rulesChanged = new();
    private readonly string _storageFolder;

    public IObservable<ImmutableArray<IgnoreRule>> RulesChanged => _rulesChanged;

    public SettingsService(IFileSystem fileSystem, ILogger<SettingsService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        var settingsFolder = _fileSystem.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Settings"
        );
        Directory.CreateDirectory(settingsFolder);
        _storageFolder = settingsFolder;

        LoadAllRules();
    }

    public bool IsIgnored(ModKey modKey, AnalyzerResultInfo resultInfo)
    {
        return GetRules(modKey)
            .Any(ignoreRule => GetIdentifier(resultInfo, ignoreRule.Type) == ignoreRule.Identifier);
    }

    public ImmutableArray<IgnoreRule> GetRules(ModKey modKey)
    {
        if (_cache.TryGetValue(modKey, out var cachedRules))
        {
            return [..cachedRules];
        }

        var filePath = GetFilePath(modKey);
        if (!_fileSystem.File.Exists(filePath))
        {
            return [];
        }

        try
        {
            var json = _fileSystem.File.ReadAllText(filePath);
            var deserializedRules = JsonSerializer.Deserialize<List<IgnoreRule>>(json) ?? [];
            _cache[modKey] = deserializedRules;
            return [..deserializedRules];
        }
        catch (Exception)
        {
            return [];
        }
    }

    public void AddRule(ModKey modKey, IgnoreRule rule)
    {
        var rulesToAdd = _cache.TryGetValue(modKey, out var existing)
            ? existing
            : GetRules(modKey).ToList();

        rulesToAdd.Add(rule);
        _cache[modKey] = rulesToAdd;
        _rulesChanged.OnNext([..rulesToAdd]);
        SaveRules(modKey, rulesToAdd);
    }

    public void AddRule(ModKey modKey, AnalyzerResultInfo resultInfo, IgnoreType ignoreType)
    {
        var identifier = GetIdentifier(resultInfo, ignoreType);
        var rule = new IgnoreRule(ignoreType, identifier);
        AddRule(modKey, rule);
    }

    public void RemoveRule(ModKey modKey, int index)
    {
        var rulesToRemove = _cache.TryGetValue(modKey, out var existing)
            ? existing
            : GetRules(modKey).ToList();

        if (index < 0 || index >= rulesToRemove.Count) return;

        rulesToRemove.RemoveAt(index);
        _cache[modKey] = rulesToRemove;
        _rulesChanged.OnNext([..rulesToRemove]);
        SaveRules(modKey, rulesToRemove);
    }

    public void ClearRules(ModKey modKey)
    {
        _cache.Remove(modKey);
        var filePath = GetFilePath(modKey);
        if (_fileSystem.File.Exists(filePath))
        {
            _fileSystem.File.Delete(filePath);
        }

        _rulesChanged.OnNext([]);
    }

    private static string GetIdentifier(AnalyzerResultInfo resultInfo, IgnoreType ignoreType)
    {
        return ignoreType switch
        {
            IgnoreType.Instance => resultInfo.GetIdentifier(),
            IgnoreType.Topic => resultInfo.Result.Topic?.TopicDefinition.Title ?? string.Empty,
            IgnoreType.Record => resultInfo.Result.Record?.FormKey.ToString() ?? string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(ignoreType))
        };
    }

    private string GetFileName(ModKey modKey)
    {
        return $"{_fileSystem.Path.GetFileName(modKey.FileName)}.json";
    }

    private string GetFilePath(ModKey modKey)
    {
        return _fileSystem.Path.Combine(_storageFolder, GetFileName(modKey));
    }

    private void SaveRules(ModKey modKey, List<IgnoreRule> rules)
    {
        try
        {
            var filePath = GetFilePath(modKey);
            var json = JsonSerializer.Serialize(new Settings(rules), new JsonSerializerOptions { WriteIndented = true });
            _fileSystem.File.WriteAllText(filePath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Failed to save settings for {ModKey}", modKey);
        }
    }

    private void LoadRules(ModKey modKey)
    {
        var filePath = GetFilePath(modKey);
        if (!_fileSystem.File.Exists(filePath))
        {
            _cache[modKey] = [];
            return;
        }

        try
        {
            var json = _fileSystem.File.ReadAllText(filePath);
            var deserializedRules = JsonSerializer.Deserialize<Settings>(json)?.Ignored ?? [];
            _cache[modKey] = deserializedRules;
        }
        catch (Exception)
        {
            _cache[modKey] = [];
        }
    }

    private void LoadAllRules()
    {
        var files = _fileSystem.Directory.GetFiles(_storageFolder, "*.json");
        foreach (var file in files)
        {
            var modKey = ModKey.FromFileName(_fileSystem.Path.GetFileNameWithoutExtension(file));
            LoadRules(modKey);
        }
    }
}