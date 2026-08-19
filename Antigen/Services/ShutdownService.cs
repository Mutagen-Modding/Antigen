using Antigen.ViewModels;
using Microsoft.Extensions.Logging;

namespace Antigen.Services;

public sealed class ShutdownService(
    GuiSettingsService guiSettings,
    GlobalSettingsVM globalSettings,
    MainVM mainVM,
    ILogger<ShutdownService> logger) : ISingleton
{
    public void Save()
    {
        logger.LogInformation("Exiting");

        guiSettings.Save(guiSettings.Current with
        {
            WindowX = mainVM.WindowX,
            WindowY = mainVM.WindowY,
            ExpandedHeight = mainVM.ExpandedHeight,
            ExpandedWidth = mainVM.ExpandedWidth,
            WorkerThreadPercentage = globalSettings.CorePercentage,
            ColorScheme = globalSettings.ColorScheme
        });
    }
}
