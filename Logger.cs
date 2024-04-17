using SparkEngine.Configuration;

namespace SparkEngine;

public static class Logger
{
    public static void Print(dynamic data, LogLevel l = LogLevel.Normal)
    {
        Overlay.Instance.Log += data.ToString();
    }

    public static void PrintLine(dynamic data, LogLevel l = LogLevel.Normal)
    {
        Overlay.Instance.Log +=
            $"{(l == LogLevel.Error ? "ERR!" : l == LogLevel.Warning ? "WARN" : "DEBG")} {data.ToString()}\n";
    }
}
public enum LogLevel
{
    Normal,
    Warning,
    Error
}