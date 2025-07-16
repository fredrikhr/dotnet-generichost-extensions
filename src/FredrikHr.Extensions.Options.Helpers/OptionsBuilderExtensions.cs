using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Options;

public static class OptionsBuilderExtensions
{
    public static IServiceCollection Inherit<TOptions, TOptionsBase>(
        this IServiceCollection services,
        string? name = default
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
        return services
            .ConfigureInherited<TOptions, TOptionsBase>(name)
            .PostConfigureInherited<TOptions, TOptionsBase>(name)
            ;
    }

    public static IServiceCollection ConfigureInherited<TOptions, TOptionsBase>(
        this IServiceCollection services,
        string? name = default
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;
        name ??= Options.DefaultName;
        services.TryAddTransient<IConfigureOptions<TOptions>>(
            serviceProvider => InheritedConfigureNamedOptions
            <TOptions, TOptionsBase>.CreateInstance(serviceProvider, name)
            );

    returnFluent:
        return services;
    }

    public static IServiceCollection PostConfigureInherited<TOptions, TOptionsBase>(
        this IServiceCollection services,
        string? name = default
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;
        name ??= Options.DefaultName;
        services.TryAddTransient<IPostConfigureOptions<TOptions>>(
            serviceProvider => InheritedPostConfigureNamedOptions
            <TOptions, TOptionsBase>.CreateInstance(serviceProvider, name)
            );

    returnFluent:
        return services;
    }

    public static IServiceCollection InheritAll<TOptions, TOptionsBase>(
        this IServiceCollection services
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
        return services
            .ConfigureInheritAll<TOptions, TOptionsBase>()
            .PostConfigureInheritAll<TOptions, TOptionsBase>()
            ;
    }

    public static IServiceCollection ConfigureInheritAll<TOptions, TOptionsBase>(
        this IServiceCollection services
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;

        services.TryAddTransient<
            IConfigureOptions<TOptions>,
            InheritedConfigureAllOptions<TOptions, TOptionsBase>
            >();

    returnFluent:
        return services;
    }

    public static IServiceCollection PostConfigureInheritAll<TOptions, TOptionsBase>(
        this IServiceCollection services
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;

        services.TryAddTransient<
            IPostConfigureOptions<TOptions>,
            InheritedPostConfigureAllOptions<TOptions, TOptionsBase>
            >();

    returnFluent:
        return services;
    }

    public static IServiceCollection ConfigureAll<TOptions>(
        this IServiceCollection services,
        Action<string?, TOptions> configureOptions
        )
        where TOptions : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<IConfigureOptions<TOptions>>(
            serviceProvider => new ConfigureAllOptions<TOptions>(
                configureOptions
                ));

        return services;
    }

    public static IServiceCollection ConfigureAll<TOptions, TDep>(
        this IServiceCollection services,
        Action<string?, TOptions, TDep> configureOptions
        )
        where TOptions : class
        where TDep : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<IConfigureOptions<TOptions>>(
            serviceProvider => new ConfigureAllOptions<TOptions, TDep>(
                serviceProvider.GetRequiredService<TDep>(),
                configureOptions
                ));

        return services;
    }

    public static IServiceCollection PostConfigureAll<TOptions>(
        this IServiceCollection services,
        Action<string?, TOptions> configureOptions
        )
        where TOptions : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<IPostConfigureOptions<TOptions>>(
            serviceProvider => new PostConfigureAllOptions<TOptions>(
                configureOptions
                ));

        return services;
    }

    public static IServiceCollection PostConfigureAll<TOptions, TDep>(
        this IServiceCollection services,
        Action<string?, TOptions, TDep> configureOptions
        )
        where TOptions : class
        where TDep : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<IPostConfigureOptions<TOptions>>(
            serviceProvider => new PostConfigureAllOptions<TOptions, TDep>(
                serviceProvider.GetRequiredService<TDep>(),
                configureOptions
                ));

        return services;
    }
}
