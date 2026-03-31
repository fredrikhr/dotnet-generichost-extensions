using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = nameof(Microsoft.Extensions.DependencyInjection)
)]
internal sealed class HostCommandExecutionService(
    IHostApplicationLifetime lifetime,
    IHostedCommandExecution invocation
    ) : BackgroundService()
{
    private Task<int>? _executeTask;
    public new Task<int>? ExecuteTask => base.ExecuteTask switch
    {
        Task<int> task => task,
        _ => _executeTask,
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _executeTask = ExecuteCoreAsync(stoppingToken);
    }

    private async Task WaitUntilApplicationStartedAsync(CancellationToken cancelToken)
    {
        TaskCompletionSource<object?> startedTcs = new();
        CancellationTokenRegistration startingCancelRegistration = cancelToken.Register(
            CancelTaskCompletionSource,
            startedTcs
            );
        CancellationTokenRegistration startedRegistration = lifetime.ApplicationStarted.Register(
            CompleteTaskCompletionSource,
            startedTcs
            );
        try
        {
            _ = await startedTcs.Task
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        finally
        {
#if NET8_0_OR_GREATER
            await startingCancelRegistration.DisposeAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            await startedRegistration.DisposeAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
#else
            startingCancelRegistration.Dispose();
            startedRegistration.Dispose();
#endif
        }
    }

    private async Task<int> ExecuteCoreAsync(CancellationToken cancelToken)
    {
        await WaitUntilApplicationStartedAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return await invocation.InvokeAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private static void CompleteTaskCompletionSource(object? state)
    {
        var tcs = (TaskCompletionSource<object?>)state!;
        tcs.SetResult(default);
    }

    private static void CancelTaskCompletionSource(object? state)
    {
        var tcs = (TaskCompletionSource<object>)state!;
#if NET6_0_OR_GREATER
            tcs.SetCanceled(CancellationToken.None);
#else
        tcs.SetCanceled();
#endif
    }
}
