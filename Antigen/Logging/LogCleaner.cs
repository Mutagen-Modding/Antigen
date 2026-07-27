using System.Globalization;
using System.IO;
using Serilog;

namespace Antigen.Logging;

public static class LogCleaner
{
    private const int DaysToKeep = 7;

    public static void Clean(string logFolder, ILogger logger)
    {
        try
        {
            var directory = new DirectoryInfo(logFolder);
            if (!directory.Exists) return;

            var cutoff = DateTime.Now.AddDays(-DaysToKeep);

            foreach (var file in directory.EnumerateFiles("*.log"))
            {
                if (file.Name == "Current.log") continue;
                if (!TryParseLogDate(file.Name, out var date) || date >= cutoff) continue;

                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to delete old log {LogFile}", file.Name);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to clean old logs in {LogFolder}", logFolder);
        }
    }

    private static bool TryParseLogDate(string fileName, out DateTime date)
    {
        date = default;
        return fileName.Length >= 10
            && DateTime.TryParseExact(fileName[..10], "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
}
