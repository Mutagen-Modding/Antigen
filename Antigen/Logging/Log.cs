using System.IO;
using Serilog;

namespace Antigen.Logging;

public static class Log
{
    public const string LogFolder = "logs";

    // Serilog's default, minus the `zzz` timezone offset
    private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static readonly ILogger Logger;
    public static readonly DateTime StartTime;

    static Log()
    {
        StartTime = DateTime.Now;

        var logFolder = Path.Combine(AppContext.BaseDirectory, LogFolder);
        Directory.CreateDirectory(logFolder);

        var logFileName = $"{StartTime:MM-dd-yyyy_HH'h'mm'm'ss's'}.log";

        var currentLog = Path.Combine(logFolder, "Current.log");
        var currentLogFailure = TryDelete(currentLog);

        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(logFolder, logFileName), outputTemplate: OutputTemplate);

        if (currentLogFailure == null)
        {
            config = config.WriteTo.File(currentLog, outputTemplate: OutputTemplate);
        }

        Serilog.Log.Logger = config.CreateLogger();
        Logger = Serilog.Log.Logger;

        if (currentLogFailure != null)
        {
            Logger.Warning(currentLogFailure, "{LogFile} is in use; logging only to {LogFileName}", currentLog, logFileName);
        }

        LogCleaner.Clean(logFolder, Logger);
    }

    private static Exception? TryDelete(string path)
    {
        try
        {
            File.Delete(path);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
