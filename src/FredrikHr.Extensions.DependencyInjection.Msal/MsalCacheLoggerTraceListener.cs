using System.Diagnostics;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed partial class MsalCacheLoggerTraceListener(
    ILogger<MsalCacheHelper> logger
    ) : TraceListener(logger.GetType().FullName!)
{
    private static readonly (TraceEventType eventType, Microsoft.Extensions.Logging.LogLevel logLevel)[] LogLevelMappings = [
        (TraceEventType.Verbose, Microsoft.Extensions.Logging.LogLevel.Debug),
        (TraceEventType.Information, Microsoft.Extensions.Logging.LogLevel.Information),
        (TraceEventType.Warning, Microsoft.Extensions.Logging.LogLevel.Warning),
        (TraceEventType.Error, Microsoft.Extensions.Logging.LogLevel.Error),
        (TraceEventType.Critical, Microsoft.Extensions.Logging.LogLevel.Critical),
        (~(TraceEventType.Verbose |
            TraceEventType.Information |
            TraceEventType.Warning |
            TraceEventType.Error |
            TraceEventType.Critical
            ), Microsoft.Extensions.Logging.LogLevel.Trace),
        ];
    private readonly AsyncLocal<StringBuilder?> _writeBuffer = new();
    private readonly ILogger<MsalCacheHelper> _logger = logger;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079: Remove unnecessary suppression", Justification = "erroneously triggered")]
    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
    {
        foreach (var tuple in LogLevelMappings)
        {
            if ((eventType & tuple.eventType) != default &&
                _logger.IsEnabled(tuple.logLevel))
            {
                _logger.Log(tuple.logLevel, id, format, args!);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079: Remove unnecessary suppression", Justification = "erroneously triggered")]
    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
    {
        foreach (var tuple in LogLevelMappings)
        {
            if ((eventType & tuple.eventType) != default &&
                _logger.IsEnabled(tuple.logLevel))
            {
                _logger.Log(tuple.logLevel, id, $"{{Message}}", [message]);
            }
        }
    }

    public override void Write(string? message)
    {
        if (message is null) return;
        var writer = _writeBuffer.Value ??= new StringBuilder();
        writer.Append(message);
    }

    public override void WriteLine(string? message)
    {
        if (message is null) return;
        var writer = _writeBuffer.Value ??= new StringBuilder();
        if (writer.Length > 0)
        {
            writer.Append(message);
            message = writer.ToString();
            _writeBuffer.Value = null;
        }
        LogTraceWriteLine(message);
    }

    [LoggerMessage(EventId = 1, EventName = nameof(Write),
        Level = Microsoft.Extensions.Logging.LogLevel.Trace,
        Message = $"{{{nameof(message)}}}"
        )]
    private partial void LogTraceWriteLine(string message);
}