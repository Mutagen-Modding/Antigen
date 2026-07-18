using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Antigen.Services.Singleton;

public interface ICrashLoggingService
{
    void LogCrash(Exception exception);
}

public sealed class CrashLoggingService(ILogger<CrashLoggingService> logger) : ICrashLoggingService
{
    public void LogCrash(Exception exception)
    {
        logger.LogCritical(exception, "{CrashLog}", BuildCrashLog(exception));
    }

    private static string BuildCrashLog(Exception exception)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Antigen Crash Log ===");
        sb.AppendLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"Processor Count: {Environment.ProcessorCount}");

        return sb.ToString();
    }
}
