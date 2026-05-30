using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

internal sealed partial class DataverseClientHostedServiceProvider
    : BackgroundService
{
    private const string TargetTypeName = "Microsoft.PowerPlatform.Dataverse.Client.Utils.ClientServiceProviders";
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly Type? _targetType;
    private readonly FieldInfo? _targetField;

    public DataverseClientHostedServiceProvider(
        IServiceProvider serviceProvider,
        ILoggerFactory? loggerFactory = null
        )
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _targetType = typeof(ServiceClient).Assembly.GetType(
            TargetTypeName,
            throwOnError: false
            );
        _targetField = _targetType?.GetField(
            name: "_instance",
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic
            );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1031: Do not catch general exception types",
        Justification = nameof(ILogger)
        )]
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _targetField?.SetValue(null, _serviceProvider);
        }
        catch (Exception fieldSettingExcept)
        {
            ILogger logger = _loggerFactory?.CreateLogger(_targetType?.FullName ?? TargetTypeName) ??
                Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            LogSetFieldFailure(logger, fieldSettingExcept switch
            {
                TargetInvocationException tiExcept => tiExcept.InnerException,
                _ => fieldSettingExcept
            });
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        EventId = 1, EventName = "StoreServiceProviderFailure",
        Message = "Failed to store .NET Generic Host Service Provider with Dataverse Client library."
        )]
    private static partial void LogSetFieldFailure(ILogger logger, Exception? exception);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1031: Do not catch general exception types",
        Justification = nameof(ILogger)
        )]
    public override void Dispose()
    {
        base.Dispose();
        if (_targetField is null) return;
        try
        {
            object? storedServiceProvider = _targetField.GetValue(null);
            if (ReferenceEquals(storedServiceProvider, _serviceProvider))
            {
                _targetField.SetValue(null, null);
            }
        }
        catch (Exception fieldSettingExcept)
        {
            ILogger logger = _loggerFactory?.CreateLogger(_targetType?.FullName ?? TargetTypeName) ??
                Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            LogSetFieldFailure(logger, fieldSettingExcept switch
            {
                TargetInvocationException tiExcept => tiExcept.InnerException,
                _ => fieldSettingExcept
            });
        }
    }
}
