using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Options;

public static class OptionsBuilderExtensions
{
    public static IServiceCollection ConfigureAllInherited<TOptions, TOptionsBase>(
        this IServiceCollection services
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;

        services.ConfigureAllNamed<TOptions, IEnumerable<IConfigureOptions<TOptionsBase>>>(
            static (name, inheritedConfigureOptions, options) =>
            {
                foreach (var configureOptions in inheritedConfigureOptions)
                {
                    switch (configureOptions)
                    {
                        case IConfigureNamedOptions<TOptionsBase> configureNamedOptions:
                            configureNamedOptions.Configure(name, options);
                            break;
                        case IConfigureOptions<TOptionsBase>
                        when name == Options.DefaultName:
                            configureOptions.Configure(options);
                            break;
                    }
                }
            });

    returnFluent:
        return services;
    }

    public static IServiceCollection PostConfigureAllInherited<TOptions, TOptionsBase>(
        this IServiceCollection services
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;

        services.PostConfigureAllNamed<TOptions, IEnumerable<IPostConfigureOptions<TOptionsBase>>>(
            static (name, inheritedConfigureOptions, options) =>
            {
                foreach (var configureOptions in inheritedConfigureOptions)
                {
                    configureOptions.PostConfigure(name, options);
                }
            });

    returnFluent:
        return services;
    }

    public static IServiceCollection ConfigureAllNamed<TOptions, TDep>(
        this IServiceCollection services,
        Action<string?, TDep, TOptions> configureOptions
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
            serviceProvider => new ConfigureAllNamedOptions<TOptions, TDep>(
                serviceProvider.GetRequiredService<TDep>(),
                configureOptions
                ));

        return services;
    }

    public static IServiceCollection PostConfigureAllNamed<TOptions, TDep>(
        this IServiceCollection services,
        Action<string?, TDep, TOptions> configureOptions
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
            serviceProvider => new PostConfigureAllNamedOptions<TOptions, TDep>(
                serviceProvider.GetRequiredService<TDep>(),
                configureOptions
                ));

        return services;
    }
}
