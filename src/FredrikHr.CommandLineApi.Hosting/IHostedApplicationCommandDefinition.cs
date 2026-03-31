using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public interface IHostedApplicationCommandDefinition
{
    Command Command { get; }

    void ConfigureHost(HostApplicationBuilder hostBuilder);
}

#if NET8_0_OR_GREATER
public interface IHostedApplicationCommandDefinition<TSelf> : IHostedApplicationCommandDefinition
    where TSelf : IHostedApplicationCommandDefinition<TSelf>
{
    static abstract TSelf Instance { get; }
}
#endif