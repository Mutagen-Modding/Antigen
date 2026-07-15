namespace Antigen.Services;

public interface ICrashLoggingService
{
    void LogCrash(Exception exception);
    string GetCrashLogsDirectory();
}