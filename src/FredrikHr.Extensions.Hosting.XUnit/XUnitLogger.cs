using Xunit;

namespace Microsoft.Extensions.Logging.XUnit;

public class XUnitLogger(string name) : ILogger
{
    private static readonly string[] LogLevels = [
        "trce",
        "debg",
        "info",
        "warn",
        "fail",
        "crit"
    ];

    public string Name { get; } = name;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
        )
    {
        if (TestContext.Current?.TestOutputHelper is not
            ITestOutputHelper output)
        {
            return;
        }

        PrepareLog(
            logLevel,
            state,
            exception,
            formatter,
            out string logLevelString,
            out string? logMessage);
        lock(output)
        {
            Log(output, logLevelString, eventId, logMessage);
        }
    }

    private static void PrepareLog<TState>(
        LogLevel logLevel,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter,
        out string logLevelString,
        out string? logMessage
        )
    {
        int logLevelInt = (int)logLevel;
        logLevelString = logLevelInt < 0 || logLevelInt >= LogLevels.Length
            ? logLevelInt.ToString("0000", System.Globalization.CultureInfo.InvariantCulture)
            : LogLevels[logLevelInt];
        logMessage = formatter?.Invoke(state, exception);
    }

    private void Log(
        ITestOutputHelper writer,
        string logLevel,
        EventId eventId,
        string? logMessage
        )
    {
        writer.Write("[");
        writer.Write(logLevel);
        writer.Write("]");
        if (Name is { Length: > 0 } categoryName)
        {
            writer.Write("[");
            writer.Write(categoryName);
            writer.Write("]");
        }
        if (eventId.ToString() is { Length: > 0 } eventString)
            writer.Write($"[{eventString}]");
        if (logMessage is not { Length: > 0 }) return;
        writer.Write(" ");
        writer.WriteLine(logMessage);
    }
}