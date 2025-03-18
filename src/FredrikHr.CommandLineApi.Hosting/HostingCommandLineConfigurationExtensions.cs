namespace System.CommandLine.Hosting;

public static class HostingCommandLineConfigurationExtensions
{
    public static CommandLineConfiguration UseHosting(
        this CommandLineConfiguration configuration
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
#else
        _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
#endif
        configuration.ProcessTerminationTimeout = null;
        return configuration;
    }
}
