using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = nameof(Microsoft.Extensions.DependencyInjection)
)]
internal sealed class HostCommandLineService(
    IHostApplicationLifetime lifetime,
    ICommandLineHostedExecution invocation
    ) : BackgroundService()
{
    public new Task<int>? ExecuteTask => base.ExecuteTask as Task<int>;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => ExecuteCoreAsync(stoppingToken);

    private async Task WaitUntilApplicationStartedAsync(CancellationToken cancelToken)
    {
        TaskCompletionSource<object?> startedTcs = new();
        using var startingCancelRegistration = cancelToken.Register(
            CancelTaskCompletionSource,
            startedTcs
            );
        using var startedRegistration = lifetime.ApplicationStarted.Register(
            CompleteTaskCompletionSource,
            startedTcs
            );
        _ = await startedTcs.Task
            .ConfigureAwait(continueOnCapturedContext: false);
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
