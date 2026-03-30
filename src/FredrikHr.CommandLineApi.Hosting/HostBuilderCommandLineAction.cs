using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostBuilderCommandLineAction<TExecution>(
    Func<string[], ParseResult, IHostBuilder>? hostBuilderFactory,
    Action<IHostBuilder> configureHostBuilder
    )
    : HostCommandLineAction<IHostBuilder, TExecution>(
        hostBuilderFactory ?? CreateDefaultHostBuidler,
        configureHostBuilder,
        static hostBuilder => hostBuilder,
        static hostBuilder => hostBuilder.Build()
        )
    where TExecution : class, ICommandLineHostedExecution
{
    public HostBuilderCommandLineAction(
        Action<IHostBuilder> configureHostBuilder
        ) : this(CreateDefaultHostBuidler, configureHostBuilder) { }

    public HostBuilderCommandLineAction(
        Func<string[], IHostBuilder>? hostBuilderFactory,
        Action<IHostBuilder> configureHostBuilder
    ) : this(
        hostBuilderFactory is not null
            ? (args, _) => hostBuilderFactory(args)
            : default,
        configureHostBuilder
        ) { }

    private static IHostBuilder CreateDefaultHostBuidler(
        string[] args,
        ParseResult _
        )
    {
        return Host.CreateDefaultBuilder(args);
    }
}
