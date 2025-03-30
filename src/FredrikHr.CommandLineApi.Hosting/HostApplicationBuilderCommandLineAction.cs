using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostApplicationBuilderCommandLineAction<TInvocation>(
    Action<HostApplicationBuilder> configureHostBuilder
    ) : HostCommandLineAction<HostApplicationBuilder, TInvocation>(
        Host.CreateApplicationBuilder,
        configureHostBuilder
        )
    where TInvocation : class, IHostCommandLineInvocation
{
    protected override void ConfigureHostServices(
        HostApplicationBuilder hostBuilder,
        Action<IServiceCollection> configureServices
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
#endif

        configureServices?.Invoke(hostBuilder.Services);
    }

    protected override void ConfigureHostConfiguration(
        HostApplicationBuilder hostBuilder,
        Action<IConfigurationBuilder> configureAction
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
#endif
        configureAction?.Invoke(hostBuilder.Configuration);
    }

    protected override IHost CreateHost(HostApplicationBuilder hostBuilder)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
#endif

        return hostBuilder.Build();
    }
}