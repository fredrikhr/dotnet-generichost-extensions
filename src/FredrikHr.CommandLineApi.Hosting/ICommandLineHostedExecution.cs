namespace System.CommandLine.Hosting;

public interface ICommandLineHostedExecution
{
    Task<int> InvokeAsync(CancellationToken cancelToken = default);
}
