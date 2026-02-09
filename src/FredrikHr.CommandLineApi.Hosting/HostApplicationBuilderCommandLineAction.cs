using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostApplicationBuilderCommandLineAction<TExecution>(
    Action<HostApplicationBuilder> configureHostBuilder
    ) : HostCommandLineAction<HostApplicationBuilder, TExecution>(
        Host.CreateApplicationBuilder,
        configureHostBuilder,
        HostApplicationBuilderUnsafeAccessor.AsHostBuilder,
        static hostBuilder => hostBuilder.Build()
        )
    where TExecution : class, ICommandLineHostedExecution
{ }
