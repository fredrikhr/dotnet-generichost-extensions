using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public interface IHostedCommandDefinition
{
    Command Command { get; }

    void ConfigureHost(IHostBuilder hostBuilder)
#if NET8_0_OR_GREATER
    {
        hostBuilder?.ConfigureServices(ConfigureServices);
    }
#else
        ;
#endif

    void ConfigureServices(HostBuilderContext context, IServiceCollection services);
}

#if NET8_0_OR_GREATER
public interface IHostedCommandDefinition<TSelf> : IHostedCommandDefinition
    where TSelf : IHostedCommandDefinition<TSelf>
{
    static abstract TSelf Instance { get; }
}

#endif