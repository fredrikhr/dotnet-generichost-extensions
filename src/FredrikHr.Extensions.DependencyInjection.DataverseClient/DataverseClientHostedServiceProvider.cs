using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

internal sealed partial class DataverseClientHostedServiceProvider(
    IServiceProvider serviceProvider,
    ILoggerFactory? loggerFactory = null
    ) : BackgroundService
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1031: Do not catch general exception types",
        Justification = nameof(ILogger)
        )]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TaskCompletionSource<object?> tcs = new();
        using var stopRegistration = stoppingToken.Register(
            CompleteTask,
            tcs
            );

        Type? targetType = typeof(ServiceClient).Assembly.GetType(
            "Microsoft.PowerPlatform.Dataverse.Client.Utils.ClientServiceProviders",
            throwOnError: false
            );
        if (targetType is null) return;

        FieldInfo? targetField = targetType.GetField(
            name: "_instance",
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic
            );
        if (targetField is null) return;

        try
        {
            targetField.SetValue(null, serviceProvider);
        }
        catch (Exception fieldSettingExcept)
        {
            var logger = loggerFactory?.CreateLogger(targetType.FullName!) ??
                Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            LogSetFieldFailure(logger, fieldSettingExcept);
            return;
        }

        await tcs.Task.ConfigureAwait(continueOnCapturedContext: false);

        object? storedServiceProvider = targetField.GetValue(serviceProvider);
        if (ReferenceEquals(storedServiceProvider, serviceProvider))
        {
            targetField.SetValue(null, null);
        }

        static void CompleteTask(object? state)
        {
            var tcs = (TaskCompletionSource<object?>)state!;
            tcs.TrySetResult(default);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        EventId = 1, EventName = "StoreServiceProviderFailure",
        Message = "Failed to store .NET Generic Host Service Provider with Dataverse Client library."
        )]
    private static partial void LogSetFieldFailure(ILogger logger, Exception exception);
}
