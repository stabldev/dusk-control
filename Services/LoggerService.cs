using System;
using System.IO;

namespace DuskControl.Services;

public static class LoggerService
{
  private static readonly string LogFilePath;
  private static readonly object _lock = new object();

  static LoggerService()
  {
    LogFilePath = Path.Combine(AppContext.BaseDirectory, "DuskControl.log");
  }

  public static void LogInfo(string message) => Log(message, "INFO");
  public static void LogWarning(string message) => Log(message, "WARNING");
  public static void LogFatal(string message) => Log(message, "FATAL");

  private static void Log(string message, string level)
  {
    lock (_lock)
    {
      try
      {
        // Simple rollover: if file is > 5MB, recreate it
        if (File.Exists(LogFilePath) && new FileInfo(LogFilePath).Length > 5 * 1024 * 1024)
        {
          File.Delete(LogFilePath);
        }

        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
        File.AppendAllText(LogFilePath, logEntry);
      }
      catch
      {
        // Fail silently if we can't write to the log (e.g. disk full, permission issue)
      }
    }
  }
}
