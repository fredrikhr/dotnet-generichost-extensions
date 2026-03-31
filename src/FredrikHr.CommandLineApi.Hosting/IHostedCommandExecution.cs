namespace System.CommandLine.Hosting;

public interface IHostedCommandExecution
{
    Task<int> InvokeAsync(CancellationToken cancelToken = default);
}
