using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostApplicationBuilderCommandLineAction<TExecution>(
    Func<string[], ParseResult, HostApplicationBuilder>? hostBuilderFactory,
    Action<HostApplicationBuilder> configureHostBuilder
    ) : HostCommandLineAction<HostApplicationBuilder, TExecution>(
        hostBuilderFactory ?? CreateDefaultHostBuidler,
        configureHostBuilder,
        HostApplicationBuilderUnsafeAccessor.AsHostBuilder,
        static hostBuilder => hostBuilder.Build()
        )
    where TExecution : class, ICommandLineHostedExecution
{
    public HostApplicationBuilderCommandLineAction(
        Action<HostApplicationBuilder> configureHostBuilder
        ) : this(CreateDefaultHostBuidler, configureHostBuilder) { }

    public HostApplicationBuilderCommandLineAction(
        Func<string[], HostApplicationBuilder>? hostBuilderFactory,
        Action<HostApplicationBuilder> configureHostBuilder
        ) : this(
            hostBuilderFactory is not null
            ? (args, _) => hostBuilderFactory(args)
            : default,
            configureHostBuilder
            ) { }

    private static HostApplicationBuilder CreateDefaultHostBuidler(
        string[] args,
        ParseResult _
        )
    {
        return Host.CreateApplicationBuilder(args);
    }
}
