namespace ScreenAutoClicker;

public enum LogLevel { Info, Debug, Warn, Error }

public class LogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public string Message { get; init; } = "";
}

public static class Logger
{
    public static event Action<LogEntry>? OnLog;

    public static void Info(string message)  => Emit(LogLevel.Info,  message);
    public static void Debug(string message) => Emit(LogLevel.Debug, message);
    public static void Warn(string message)  => Emit(LogLevel.Warn,  message);
    public static void Error(string message) => Emit(LogLevel.Error, message);

    private static void Emit(LogLevel level, string message)
        => OnLog?.Invoke(new LogEntry { Timestamp = DateTime.Now, Level = level, Message = message });
}
