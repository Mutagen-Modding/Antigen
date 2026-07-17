using System.IO.Abstractions;
using System.Text.Json;
using Antigen.Models.Settings;
using Microsoft.Extensions.Logging;

namespace Antigen.Services;

public sealed class GuiSettingsService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<GuiSettingsService> _logger;
    private readonly string _filePath;

    public GuiSettingsService(IFileSystem fileSystem, ILogger<GuiSettingsService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        var settingsFolder = _fileSystem.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Settings"
        );
        _fileSystem.Directory.CreateDirectory(settingsFolder);
        _filePath = _fileSystem.Path.Combine(settingsFolder, "GuiSettings.json");
    }

    public GuiSettings? Load()
    {
        if (!_fileSystem.File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            var json = _fileSystem.File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<GuiSettings>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void Save(GuiSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            // Write to a temp file then swap, so a crash mid-write can't corrupt the settings.
            var tempPath = _filePath + ".tmp";
            _fileSystem.File.WriteAllText(tempPath, json);
            _fileSystem.File.Move(tempPath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Failed to save GUI settings");
        }
    }
}
