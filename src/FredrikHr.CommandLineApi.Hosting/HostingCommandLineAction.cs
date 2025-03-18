using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public abstract class HostingCommandLineAction
    : AsynchronousCommandLineAction
{
    internal HostingCommandLineAction() : base() { }

    internal Action<IServiceCollection>? ConfigureAdditionalServices { get; set; }
}

public abstract class HostingCommandLineAction<TBuilder, TInvocation>(
    Func<string[], TBuilder> hostBuilderFactory,
    Action<TBuilder> configureHostBuilder
    )
    : HostingCommandLineAction()
    where TInvocation : class, IHostedCommandLineInvocation
{
    public Action<TBuilder> ConfigureBuilder { get; } = configureHostBuilder;

    protected abstract IHost CreateHost(TBuilder hostBuilder);

    protected abstract void ConfigureHostServices(TBuilder hostBuilder,
        Action<IServiceCollection> configureServices);

    protected abstract void ConfigureHostConfiguration(TBuilder hostBuilder,
        Action<IConfigurationBuilder> configureAction);

    public override sealed async Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(parseResult);
#else
        _ = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
#endif

        string[] unmatchedTokens = parseResult.UnmatchedTokens?.ToArray() ?? [];
        TBuilder hostBuilder = hostBuilderFactory(unmatchedTokens);

        if (parseResult.Configuration.RootCommand is RootCommand rootCommand &&
            rootCommand.Directives.FirstOrDefault(
                d => string.Equals(
                    d.Name,
                    HostingConfigurationDirective.Name,
                    StringComparison.OrdinalIgnoreCase
                    )
                ) is Directive configDirective &&
            parseResult.GetResult(configDirective) is DirectiveResult configResult
            )
        {
            var configKvps = configResult.Values
                .Select(kvpString =>
                {
                    ReadOnlySpan<char> kvpSpan = kvpString.AsSpan();
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
                }).ToList();
            ConfigureHostConfiguration(
                hostBuilder,
                (config) => config.AddInMemoryCollection(configKvps)
                );
        }

        ConfigureHostServices(hostBuilder, services =>
        {
            services.AddSingleton(parseResult);
            services.AddSingleton(parseResult.Configuration);
            services.AddSingleton<
                IHostedCommandLineInvocation,
                TInvocation
                >();
            services.AddHostedService<HostedCommandLineService>();
            ConfigureAdditionalServices?.Invoke(services);
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

    private static async Task<int> GetInvocationExecutionResult(
        IHost host, CancellationToken cancelToken
        )
    {
        IServiceProvider serviceProvider = host.Services;
        var invocationTasks = serviceProvider.GetServices<IHostedService>()
            .OfType<HostedCommandLineService>()
            .Select(s => s.ExecuteTask!).ToList();
        int[] invocationResults;
        try
        {
            invocationResults = await Task.WhenAll(invocationTasks)
            .ConfigureAwait(continueOnCapturedContext: false);
        }
        finally
        {
            invocationResults = [..invocationTasks.Where(t =>
#if NET6_0_OR_GREATER
                t.IsCompletedSuccessfully
#else
                t.IsCompleted && !(t.IsCanceled || t.IsFaulted)
#endif
            )
            .Select(t => t.Result)];
        }
        await host.StopAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        return invocationResults.FirstOrDefault(r => r != default);
    }
}