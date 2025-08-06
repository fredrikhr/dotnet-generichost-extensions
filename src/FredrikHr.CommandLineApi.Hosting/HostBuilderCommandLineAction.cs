using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostBuilderCommandLineAction<TInvocation>(
    Func<string[], IHostBuilder>? hostBuilderFactory,
    Action<IHostBuilder> configureHostBuilder
    )
    : HostCommandLineAction<IHostBuilder, TInvocation>(
        hostBuilderFactory ?? Host.CreateDefaultBuilder,
        configureHostBuilder,
        static hostBuilder => hostBuilder
        )
    where TInvocation : class, IHostCommandLineInvocation
{
    public HostBuilderCommandLineAction(
        Action<IHostBuilder> configureHostBuilder
    ) : this(default, configureHostBuilder) { }
}
