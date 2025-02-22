using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed partial class MsalCacheLoggerTraceListener(
    ILogger<MsalCacheHelper> logger
    ) : TraceListener(logger.GetType().FullName!)
{
    private readonly ILogger<MsalCacheHelper> _logger = logger;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
    {
        Span<(TraceEventType eventType, Microsoft.Extensions.Logging.LogLevel logLevel)> mappings = stackalloc[]
        {
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
        };
        foreach (ref readonly var tuple in mappings)
        {
            if ((eventType & tuple.eventType) != default &&
                _logger.IsEnabled(tuple.logLevel))
            {
                _logger.Log(tuple.logLevel, id, format, args!);
            }
        }
    }

    public override void Write(string? message)
    {
        if (message is null) return;
        LogTraceWrite(message);
    }

    [LoggerMessage(EventId = 1, EventName = nameof(Write),
        Level = Microsoft.Extensions.Logging.LogLevel.Trace,
        Message = $"{{{nameof(message)}}}"
        )]
    private partial void LogTraceWrite(string message);

    public override void WriteLine(string? message)
    {
        if (message is null) return;
        LogTraceWriteLine(message);
    }

    [LoggerMessage(EventId = 2, EventName = nameof(WriteLine),
        Level = Microsoft.Extensions.Logging.LogLevel.Trace,
        Message = $"{{{nameof(message)}}}"
        )]
    private partial void LogTraceWriteLine(string message);
}