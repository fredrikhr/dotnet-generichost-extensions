namespace Microsoft.Extensions.Logging.TUnit;

public class TUnitLogger(string name) : ILogger
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
        switch (Context.Current)
        {
            case TestContext testContext:
                Log(testContext, logLevel, eventId, state, exception, formatter);
                break;
            case var context:
                Log(context, logLevel, eventId, state, exception, formatter);
                break;
        }
    }

    private void Log<TState>(
        TestContext context,
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
        )
    {
        PrepareLog(
            logLevel,
            state,
            exception,
            formatter,
            out string logLevelString,
            out string? logMessage
            );
        lock (context.Lock)
        {
            TextWriter writer = context.OutputWriter;
            Log(writer, logLevelString, eventId, logMessage);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2002: Do not lock on objects with weak identity",
        Justification = nameof(Context)
        )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0079: Remove unnecessary suppression"
        )]
    private void Log<TState>(
        Context context,
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
        )
    {
        PrepareLog(
            logLevel,
            state,
            exception,
            formatter,
            out string logLevelString,
            out string? logMessage
            );
        TextWriter writer;
        lock (writer = context.OutputWriter)
        {
            Log(writer, logLevelString, eventId, logMessage);
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
        TextWriter writer,
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