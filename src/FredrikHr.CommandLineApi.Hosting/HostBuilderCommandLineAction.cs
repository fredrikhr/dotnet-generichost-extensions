using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostBuilderCommandLineAction<TExecution>(
    Func<string[], IHostBuilder>? hostBuilderFactory,
    Action<IHostBuilder> configureHostBuilder
    )
    : HostCommandLineAction<IHostBuilder, TExecution>(
        hostBuilderFactory ?? Host.CreateDefaultBuilder,
        configureHostBuilder,
        static hostBuilder => hostBuilder,
        static hostBuilder => hostBuilder.Build()
        )
    where TExecution : class, ICommandLineHostedExecution
{
    public HostBuilderCommandLineAction(
        Action<IHostBuilder> configureHostBuilder
    ) : this(default, configureHostBuilder) { }
}
