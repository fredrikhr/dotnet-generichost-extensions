namespace System.CommandLine.Hosting;

internal sealed class InlineCommandLineHostedExecution(
    IServiceProvider serviceProvider,
    Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
    ) : IHostedCommandExecution
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ??
        throw new ArgumentNullException(nameof(serviceProvider));
    private readonly Func<IServiceProvider, CancellationToken, Task<int>> _invokeAsync = invokeAsync ??
        throw new ArgumentNullException(nameof(invokeAsync));

    public Task<int> InvokeAsync(CancellationToken cancelToken = default)
    {
        return _invokeAsync(_serviceProvider, cancelToken);
    }
}