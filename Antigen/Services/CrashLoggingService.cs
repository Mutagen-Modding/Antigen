using System.Runtime.InteropServices;
using System.Text;

namespace Antigen.Services;

public sealed class CrashLoggingService : ICrashLoggingService
{
    private readonly string _crashLogsDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "crashes"
    );

    public void LogCrash(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(_crashLogsDirectory);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var logFileName = $"crash-{timestamp}.log";
            var logFilePath = Path.Combine(_crashLogsDirectory, logFileName);

            var logContent = BuildCrashLog(exception);

            File.WriteAllText(logFilePath, logContent, Encoding.UTF8);
        }
        catch
        {
            // If logging itself fails, we can't do much about it
        }
    }

    public string GetCrashLogsDirectory()
    {
        return _crashLogsDirectory;
    }

    private static string BuildCrashLog(Exception exception)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Antigen Crash Log ===");
        sb.AppendLine($"Timestamp: {DateTime.Now:O}");
        sb.AppendLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"Processor Count: {Environment.ProcessorCount}");
        sb.AppendLine();

        AppendExceptionDetails(sb, exception, 0);

        return sb.ToString();
    }

    private static void AppendExceptionDetails(StringBuilder sb, Exception ex, int depth)
    {
        while (true)
        {
            var indent = new string(' ', depth * 2);

            if (depth == 0)
            {
                sb.AppendLine("=== Exception ===");
            }
            else
            {
                sb.AppendLine($"{indent}--- Inner Exception ---");
            }

            sb.AppendLine($"{indent}Type: {ex.GetType().FullName}");
            sb.AppendLine($"{indent}Message: {ex.Message}");
            sb.AppendLine($"{indent}HResult: 0x{ex.HResult:X8}");
            sb.AppendLine();

            sb.AppendLine($"{indent}Stack Trace:");
            if (string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                sb.AppendLine($"{indent}  (no stack trace available)");
            }
            else
            {
                foreach (var line in ex.StackTrace.Split('\n'))
                {
                    sb.AppendLine($"{indent}  {line}");
                }
            }
            sb.AppendLine();

            if (ex.InnerException is not null)
            {
                ex = ex.InnerException;
                depth += 1;
                continue;
            }
            break;
        }
    }
}