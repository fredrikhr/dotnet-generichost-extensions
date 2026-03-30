using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public abstract class HostCommandLineAction
    : AsynchronousCommandLineAction
{
    internal HostCommandLineAction() : base() { }

    internal Action<IServiceCollection>? ConfigureSymbolServices { get; set; }
}

public abstract class HostCommandLineAction<TBuilder, TExecution>(
    Func<string[], TBuilder> hostBuilderFactory,
    Action<TBuilder> configureHostBuilder,
    Func<TBuilder, IHostBuilder> builderAsHostBuilder,
    Func<TBuilder, IHost> hostBuilderBuild
    ) : HostCommandLineAction()
    where TExecution : class, ICommandLineHostedExecution
{
    private readonly Func<TBuilder, IHostBuilder> _builderAsHostBuilder =
        builderAsHostBuilder ?? throw new ArgumentNullException(nameof(builderAsHostBuilder));

    public Action<TBuilder> ConfigureBuilder { get; } = configureHostBuilder;

    public override sealed async Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(parseResult);
#else
        _ = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
#endif

        string[] unmatchedTokens = parseResult.UnmatchedTokens?.ToArray() ?? [];
        TBuilder typedBuilder = hostBuilderFactory(unmatchedTokens);
        IHostBuilder hostBuilder = _builderAsHostBuilder(typedBuilder);
        hostBuilder.Properties[typeof(ParseResult)] = parseResult;

        // As long as done before first await,
        // configuration modification is respected
        parseResult.InvocationConfiguration.ProcessTerminationTimeout = null;

        TryApplyHostConfigurationDirective(parseResult, hostBuilder);

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(parseResult);
            services.AddSingleton(parseResult.Configuration);
            services.AddSingleton<
                ICommandLineHostedExecution,
                TExecution
                >();
            services.AddHostedService<HostCommandLineService>();
            ConfigureSymbolServices?.Invoke(services);
        });

        ConfigureBuilder?.Invoke(typedBuilder);

        using var host = hostBuilderBuild is not null
            ? hostBuilderBuild(typedBuilder)
            : hostBuilder.Build();
        await host.StartAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        var resultTask = GetExecutionResult(host, cancellationToken);

        await host.WaitForShutdownAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return await resultTask
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private static void TryApplyHostConfigurationDirective(
        ParseResult parseResult, IHostBuilder hostBuilder
        )
    {
        if (parseResult.RootCommandResult.Command is RootCommand rootCommand &&
            rootCommand.Directives.FirstOrDefault(IsConfigDirective)
                is Directive configDirective &&
            parseResult.GetResult(configDirective) is DirectiveResult configResult
            )
        {
            var configKvps = configResult.Values.Select(GetKeyValuePair)
                .ToList();
            hostBuilder.ConfigureHostConfiguration(
                (config) => config.AddInMemoryCollection(configKvps)
                );
        }

        static bool IsConfigDirective(Directive directive) => string.Equals(
            directive.Name,
            HostConfigurationDirective.Name,
            StringComparison.OrdinalIgnoreCase
        );

        static KeyValuePair<string, string?> GetKeyValuePair(string configDirective)
        {
            ReadOnlySpan<char> kvpSpan = configDirective.AsSpan();
            int eqlIdx = kvpSpan.IndexOf('=');
            string key;
            string? value = default;
            if (eqlIdx < 0)
                key = kvpSpan.Trim().ToString();
            else
            {
                key = kvpSpan[..eqlIdx].Trim().ToString();
                value = kvpSpan[(eqlIdx + 1)..].Trim().ToString();
            }
            return new KeyValuePair<string, string?>(key, value);
        }
    }

    private static async Task<int> GetExecutionResult(
        IHost host, CancellationToken cancelToken
        )
    {
        IServiceProvider serviceProvider = host.Services;
        var executeTasks = serviceProvider.GetServices<IHostedService>()
            .OfType<HostCommandLineService>()
            .Select(s => s.ExecuteTask!).ToList();
        int[] invocationResults;
        try
        {
            invocationResults = await Task.WhenAll(executeTasks)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        finally
        {
            invocationResults = [
                ..executeTasks.Where(IsTaskSuccessful).Select(t => t.Result)
            ];
        }
        await host.StopAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        return invocationResults.FirstOrDefault(r => r != default);

        static bool IsTaskSuccessful(Task t)
        {
#if NET6_0_OR_GREATER
            return t.IsCompletedSuccessfully;
#else
            return t.IsCompleted && !(t.IsCanceled || t.IsFaulted);
#endif
        }
    }
}