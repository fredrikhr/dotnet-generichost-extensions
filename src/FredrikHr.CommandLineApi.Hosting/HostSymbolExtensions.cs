using System.CommandLine.Parsing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Hosting;

public static class HostSymbolExtensions
{
    private static void ConfigureOptionsBuilder<T, TOptions>(
        Symbol symbol,
        string? optionsName,
        Action<
            OptionsBuilder<TOptions>,
            Action<TOptions, ParseResult>
            > configureOptionsBuilder,
        Action<TOptions, T?> configureOptionsInstance
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
            T? symbolValue = (symbol, symbolResult) switch
            {
                (Argument<T> argument, ArgumentResult argumentResult) =>
                argumentResult.GetValue(argument),
                (Option<T> option, OptionResult optionResult) =>
                optionResult.GetValue(option),
                _ => throw new InvalidOperationException()
            };
            configureOptionsInstance?.Invoke(options, symbolValue);
        }
    }

    public static Option<T> Configure<T, TOptions>(
        this Option<T> option,
        Action<TOptions, T?> configureOptions
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

    public static Option<T> Configure<T, TOptions>(
        this Option<T> option,
        string? optionsName,
        Action<TOptions, T?> configureOptions
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

    public static Option<T> PostConfigure<T, TOptions>(
        this Option<T> option,
        Action<TOptions, T?> configureOptions
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

    public static Option<T> PostConfigure<T, TOptions>(
        this Option<T> option,
        string? optionsName,
        Action<TOptions, T?> configureOptions
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
        where TExecution : class, ICommandLineHostedExecution
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
                ICommandLineHostedExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync)));
        }
    }

    public static Command UseHostExecution<TExecution>(
        this Command command,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, ICommandLineHostedExecution
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
        where TInvocation : class, IHostCommandLineInvocation
    {
        UseHostExecution<InlineCommandLineHostedExecution>(
            command,
            configureHost + AddInvocationSingleton
            );
        return command;
    }

        void AddInvocationSingleton(IHostBuilder builder)
    {
            builder.ConfigureServices(services => services.AddSingleton<
                ICommandLineHostedExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync)));
        }
    }

    public static Command UseHostExecution<TExecution>(
        this Command command,
        Action<HostApplicationBuilder> configureHost
        )
        where TExecution : class, ICommandLineHostedExecution
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
                ICommandLineHostedExecution,
                InlineCommandLineHostedExecution
                >(sp => new(sp, invokeAsync));
        }
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Func<string[], IHostBuilder> createHostBuilder,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, ICommandLineHostedExecution
    {
        UseHostExecution<TExecution>((Command)command, createHostBuilder, configureHost);
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
        return command;
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Action<IHostBuilder> configureHost
        )
        where TExecution : class, ICommandLineHostedExecution
    {
        UseHostExecution<TExecution>((Command)command, configureHost);
        return command;
    }

    public static RootCommand UseHostExecution(
        this RootCommand command,
        Action<IHostBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution((Command)command, configureHost, invokeAsync);
        return command;
    }

    public static RootCommand UseHostExecution<TExecution>(
        this RootCommand command,
        Action<HostApplicationBuilder> configureHost
        )
        where TExecution : class, ICommandLineHostedExecution
    {
        UseHostExecution<TExecution>((Command)command, configureHost);
        return command;
    }

    public static RootCommand UseHostExecution(
        this RootCommand command,
        Action<HostApplicationBuilder> configureHost,
        Func<IServiceProvider, CancellationToken, Task<int>> invokeAsync
        )
    {
        UseHostExecution((Command)command, configureHost, invokeAsync);
        return command;
    }
}