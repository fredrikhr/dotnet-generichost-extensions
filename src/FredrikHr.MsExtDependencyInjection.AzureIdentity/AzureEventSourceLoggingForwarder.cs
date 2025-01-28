using System.Collections.Concurrent;
using System.Diagnostics.Tracing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Core.Diagnostics;

internal sealed class AzureEventSourceLoggingForwarder : ILoggerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private ILoggerFactory? _loggerFactory;
    private readonly AzureEventSourceListener _listener;
    private readonly Func<string, ILogger> _loggerCreate;
    private readonly ConcurrentDictionary<string, ILogger> _loggerCache =
        new(StringComparer.OrdinalIgnoreCase);

    public AzureEventSourceLoggingForwarder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _listener = new(OnEventWritten, EventLevel.LogAlways);
        _loggerCreate = CreateLogger;
    }

    private ILogger CreateLogger(string eventSourceName)
    {
        _loggerFactory ??= _serviceProvider.GetService<ILoggerFactory>() ??
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
        return _loggerFactory.CreateLogger(eventSourceName);
    }

    private void OnEventWritten(EventWrittenEventArgs args, string message)
    {
        var logger = _loggerCache.GetOrAdd(args.EventSource.Name, _loggerCreate);
        LogLevel logLevel = ToLogLevel(args.Level);
        if (!logger.IsEnabled(logLevel)) return;
        EventId eventId = ToEventId(args);
        logger.Log(logLevel, eventId, args, exception: null, (_, _) => message);
    }

    private static LogLevel ToLogLevel(EventLevel eventLevel)
    {
        const int max = (int)LogLevel.None;
        var value = (int)eventLevel;
        return (LogLevel)(max - value);
    }

    private static EventId ToEventId(EventWrittenEventArgs args) =>
        new(args.EventId, args.EventName);

    ILogger ILoggerProvider.CreateLogger(string _) => Microsoft.Extensions
        .Logging.Abstractions.NullLogger.Instance;

    public void Dispose()
    {
        _listener.Dispose();
        _loggerCache.Clear();
    }
}
