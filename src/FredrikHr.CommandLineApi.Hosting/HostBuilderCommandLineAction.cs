using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostBuilderCommandLineAction<TInvocation>(
    Func<string[], IHostBuilder>? hostBuilderFactory,
    Action<IHostBuilder> configureHostBuilder
    )
    : HostCommandLineAction<IHostBuilder, TInvocation>(
        hostBuilderFactory ?? Host.CreateDefaultBuilder,
        configureHostBuilder
        )
    where TInvocation : class, IHostCommandLineInvocation
{
    public HostBuilderCommandLineAction(
        Action<IHostBuilder> configureHostBuilder
    ) : this(default, configureHostBuilder) { }

    protected override void ConfigureHostServices(
        IHostBuilder hostBuilder,
        Action<IServiceCollection> configureServices
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
#endif
        hostBuilder.ConfigureServices((c, s) => configureServices(s));
    }

    protected override void ConfigureHostConfiguration(
        IHostBuilder hostBuilder,
        Action<IConfigurationBuilder> configureAction
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
#endif
        hostBuilder.ConfigureHostConfiguration(configureAction);
        hostBuilder.UseConsoleLifetime();
    }

    protected override IHost CreateHost(IHostBuilder hostBuilder)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hostBuilder);
#else
        _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
#endif
        return hostBuilder.Build();
    }
}
