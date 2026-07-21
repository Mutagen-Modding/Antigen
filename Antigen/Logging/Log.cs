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
        if (File.Exists(currentLog))
        {
            File.Delete(currentLog);
        }

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(logFolder, logFileName), outputTemplate: OutputTemplate)
            .WriteTo.File(currentLog, outputTemplate: OutputTemplate)
            .CreateLogger();

        Logger = Serilog.Log.Logger;

        LogCleaner.Clean(logFolder, Logger);
    }
}
