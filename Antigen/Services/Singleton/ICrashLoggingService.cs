namespace Antigen.Services.Singleton;

public interface ICrashLoggingService
{
    void LogCrash(Exception exception);
    string GetCrashLogsDirectory();
}