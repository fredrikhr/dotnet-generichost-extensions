namespace System.CommandLine.Hosting;

public interface IHostedCommandLineInvocation
{
    Task<int> InvokeAsync(CancellationToken cancelToken = default);
}
