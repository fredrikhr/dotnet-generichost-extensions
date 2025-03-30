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

public abstract class HostCommandLineAction<TBuilder, TInvocation>(
    Func<string[], TBuilder> hostBuilderFactory,
    Action<TBuilder> configureHostBuilder
    ) : HostCommandLineAction()
    where TInvocation : class, IHostCommandLineInvocation
{
    public Action<TBuilder> ConfigureBuilder { get; } = configureHostBuilder;

    protected abstract IHost CreateHost(TBuilder hostBuilder);

    protected abstract void ConfigureHostServices(TBuilder hostBuilder,
        Action<IServiceCollection> configureServices);

    protected abstract void ConfigureHostConfiguration(TBuilder hostBuilder,
        Action<IConfigurationBuilder> configureAction);

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
        TBuilder hostBuilder = hostBuilderFactory(unmatchedTokens);

        // As long as done before first await,
        // configuration modification is respected
        parseResult.Configuration.ProcessTerminationTimeout = null;

        TryApplyHostConfigurationDirective(parseResult, hostBuilder);

        ConfigureHostServices(hostBuilder, services =>
        {
            services.AddSingleton(parseResult);
            services.AddSingleton(parseResult.Configuration);
            services.AddSingleton<
                IHostCommandLineInvocation,
                TInvocation
                >();
            services.AddHostedService<HostCommandLineService>();
            ConfigureSymbolServices?.Invoke(services);
        });

        ConfigureBuilder?.Invoke(hostBuilder);

        using var host = CreateHost(hostBuilder);
        await host.StartAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        var resultTask = GetInvocationExecutionResult(host, cancellationToken);

        await host.WaitForShutdownAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        return await resultTask
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private void TryApplyHostConfigurationDirective(
        ParseResult parseResult, TBuilder hostBuilder
        )
    {
        if (parseResult.Configuration.RootCommand is RootCommand rootCommand &&
            rootCommand.Directives.FirstOrDefault(IsConfigDirective)
                is Directive configDirective &&
            parseResult.GetResult(configDirective) is DirectiveResult configResult
            )
        {
            var configKvps = configResult.Values.Select(GetKeyValuePair)
                .ToList();
            ConfigureHostConfiguration(
                hostBuilder,
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

    private static async Task<int> GetInvocationExecutionResult(
        IHost host, CancellationToken cancelToken
        )
    {
        IServiceProvider serviceProvider = host.Services;
        var invocationTasks = serviceProvider.GetServices<IHostedService>()
            .OfType<HostCommandLineService>()
            .Select(s => s.ExecuteTask!).ToList();
        int[] invocationResults;
        try
        {
            invocationResults = await Task.WhenAll(invocationTasks)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        finally
        {
            invocationResults = [
                ..invocationTasks.Where(IsTaskSuccessful).Select(t => t.Result)
            ];

            static bool IsTaskSuccessful(Task t) =>
#if NET6_0_OR_GREATER
                    t.IsCompletedSuccessfully
#else
                    t.IsCompleted && !(t.IsCanceled || t.IsFaulted)
#endif
                ;
        }
        await host.StopAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        return invocationResults.FirstOrDefault(r => r != default);
    }
}