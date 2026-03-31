using System.CommandLine.Parsing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Hosting;

public static class HostSymbolExtensions
{
    private static void ConfigureOptionsBuilder<TOptions, TValue>(
        Symbol symbol,
        string? optionsName,
        Action<
            OptionsBuilder<TOptions>,
            Action<TOptions, ParseResult>
            > configureOptionsBuilder,
        Action<TOptions, TValue?> configureOptionsInstance
        ) where TOptions : class
    {
        switch (symbol)
        {
            case Argument argument:
                argument.Validators.Add(SymbolResultAction);
                break;
            case Option option:
                option.Validators.Add(SymbolResultAction);
                break;
            default:
                throw new InvalidOperationException();
        }

        void SymbolResultAction(SymbolResult symbolResult)
        {
            CommandResult? commandResult = null;
            for (SymbolResult? parentResult = symbolResult;
                parentResult is not null &&
                (commandResult = parentResult as CommandResult) is null;
                parentResult = parentResult.Parent) ;
            if (commandResult is null) return;
            if (commandResult.Command.Action is not HostCommandLineAction hostingAction)
                return;
            hostingAction.ConfigureSymbolServices += ConfigureHostServices;
        }

        void ConfigureHostServices(IServiceCollection services)
        {
            var optionsBuilder = services.AddOptions<TOptions>(optionsName);
            configureOptionsBuilder(optionsBuilder, ConfigureOptionsInstance);
        }

        void ConfigureOptionsInstance(TOptions options, ParseResult parseResult)
        {
            var symbolResult = parseResult.GetResult(symbol);
            if (symbolResult is null) return;
            TValue? symbolValue = (symbol, symbolResult) switch
            {
                (Argument<TValue> argument, ArgumentResult argumentResult) =>
                argumentResult.GetValue(argument),
                (Option<TValue> option, OptionResult optionResult) =>
                optionResult.GetValue(option),
                _ => throw new InvalidOperationException()
            };
            configureOptionsInstance?.Invoke(options, symbolValue);
        }
    }

    public static Option<TValue> Configure<TOptions, TValue>(
        this Option<TValue> option,
        Action<TOptions, TValue?> configureOptions
        ) where TOptions : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(option);
#else
        _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
        ConfigureOptionsBuilder(
            option,
            default,
            static (builder, configureOptionsAction) =>
                builder.Configure(configureOptionsAction),
            configureOptions
            );
        return option;
    }

    public static Option<TValue> Configure<TOptions, TValue>(
        this Option<TValue> option,
        string? optionsName,
        Action<TOptions, TValue?> configureOptions
        ) where TOptions : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(option);
#else
        _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
        ConfigureOptionsBuilder(
            option,
            optionsName,
            static (builder, configureOptionsAction) =>
                builder.Configure(configureOptionsAction),
            configureOptions
            );
        return option;
    }

    public static Option<TValue> PostConfigure<TOptions, TValue>(
        this Option<TValue> option,
        Action<TOptions, TValue?> configureOptions
        ) where TOptions : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(option);
#else
        _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
        ConfigureOptionsBuilder(
            option,
            default,
            static (builder, configureOptionsAction) =>
                builder.PostConfigure(configureOptionsAction),
            configureOptions
            );
        return option;
    }

    public static Option<TValue> PostConfigure<TOptions, TValue>(
        this Option<TValue> option,
        string? optionsName,
        Action<TOptions, TValue?> configureOptions
        ) where TOptions : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(option);
#else
        _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
        ConfigureOptionsBuilder(
            option,
            optionsName,
            static (builder, configureOptionsAction) =>
                builder.PostConfigure(configureOptionsAction),
            configureOptions
            );
        return option;
    }

    public static Command UseHostExecution<TExecution>(
        this Command command,
        Func<string[], IHostBuilder>? createHostBuilder,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(command);
#else
        _ = command ?? throw new ArgumentNullException(nameof(command));
#endif
        command.Action = new HostBuilderCommandLineAction<TExecution>(
            createHostBuilder,
            configureHost
            );
        return command;
    }

    public static Command UseHostExecution<TExecution>(
        this Command command,
        Func<string[], ParseResult, IHostBuilder>? createHostBuilder,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(command);
#else
        _ = command ?? throw new ArgumentNullException(nameof(command));
#endif
        command.Action = new HostBuilderCommandLineAction<TExecution>(
            createHostBuilder,
            configureHost
            );
        return command;
    }

    public static Command UseHostExecution(
        this Command command,
        Func<string[], IHostBuilder>? createHostBuilder,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution<InlineCommandLineHostedExecution>(
            command,
            createHostBuilder,
            configureHost + AddInvocationSingleton
            );
        return command;

        void AddInvocationSingleton(IHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddSingleton<
                IHostedCommandExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync)));
        }
    }

    public static Command UseHostExecution(
        this Command command,
        Func<string[], ParseResult, IHostBuilder>? createHostBuilder,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution<InlineCommandLineHostedExecution>(
            command,
            createHostBuilder,
            configureHost + AddInvocationSingleton
            );
        return command;

        void AddInvocationSingleton(IHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddSingleton<
                IHostedCommandExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync)));
        }
    }

    public static Command UseHostExecution<TExecution>(
        this Command command,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(command);
#else
        _ = command ?? throw new ArgumentNullException(nameof(command));
#endif
        command.Action = new HostBuilderCommandLineAction<TExecution>(
            configureHost
            );
        return command;
    }

    public static Command UseHostExecution(
        this Command command,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution<InlineCommandLineHostedExecution>(
            command,
            configureHost + AddInvocationSingleton
            );
        return command;

        void AddInvocationSingleton(IHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddSingleton<
                IHostedCommandExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync)));
        }
    }

    public static Command UseHostExecution<TExecution>(
        this Command command,
        Action<HostApplicationBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(command);
#else
        _ = command ?? throw new ArgumentNullException(nameof(command));
#endif
        command.Action = new HostApplicationBuilderCommandLineAction<TExecution>(
            configureHost
            );
        return command;
    }

    public static Command UseHostExecution(
        this Command command,
        Action<HostApplicationBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution<InlineCommandLineHostedExecution>(
            command,
            configureHost + AddInvocationSingleton
            );
        return command;

        void AddInvocationSingleton(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<
                IHostedCommandExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync));
        }
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Func<string[], IHostBuilder> createHostBuilder,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
        UseHostExecution<TExecution>((Command)command, createHostBuilder, configureHost);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Func<string[], ParseResult, IHostBuilder> createHostBuilder,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
        UseHostExecution<TExecution>((Command)command, createHostBuilder, configureHost);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution(
        this RootCommand command,
        Func<string[], IHostBuilder> createHostBuilder,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution((Command)command, createHostBuilder, configureHost, invokeAsync);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution(
        this RootCommand command,
        Func<string[], ParseResult, IHostBuilder> createHostBuilder,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution((Command)command, createHostBuilder, configureHost, invokeAsync);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
        UseHostExecution<TExecution>((Command)command, configureHost);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution(
        this RootCommand command,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution((Command)command, configureHost, invokeAsync);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Action<HostApplicationBuilder> configureHost
        )
        where TExecution : class, IHostedCommandExecution
    {
        UseHostExecution<TExecution>((Command)command, configureHost);
        command.ApplyHostExecutionSettings();
        return command;
    }

    public static RootCommand UseHostExecution(
        this RootCommand command,
        Action<HostApplicationBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution((Command)command, configureHost, invokeAsync);
        command.ApplyHostExecutionSettings();
        return command;
    }

    private static void ApplyHostExecutionSettings(
        this RootCommand? command
        )
    {
        if (command is null) return;
        command.TreatUnmatchedTokensAsErrors = false;
        command.Directives.Add(new EnvironmentVariablesDirective());
        command.Directives.Add(new HostConfigurationDirective());
    }
}