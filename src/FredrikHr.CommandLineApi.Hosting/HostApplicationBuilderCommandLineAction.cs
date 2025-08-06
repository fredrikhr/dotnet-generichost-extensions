using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostApplicationBuilderCommandLineAction<TInvocation>(
    Action<HostApplicationBuilder> configureHostBuilder
    ) : HostCommandLineAction<HostApplicationBuilder, TInvocation>(
        Host.CreateApplicationBuilder,
        configureHostBuilder,
        HostApplicationBuilderUnsafeAccessor.AsHostBuilder
        )
    where TInvocation : class, IHostCommandLineInvocation
{ }
