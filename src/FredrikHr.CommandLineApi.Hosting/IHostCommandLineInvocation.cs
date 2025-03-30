namespace System.CommandLine.Hosting;

public interface IHostCommandLineInvocation
{
    Task<int> InvokeAsync(CancellationToken cancelToken = default);
}
