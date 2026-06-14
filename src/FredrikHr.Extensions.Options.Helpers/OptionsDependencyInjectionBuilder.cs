using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Options;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class OptionsDependencyInjectionBuilder
{
    extension(IServiceCollection services)
    {
        public IServiceCollection Inherit<TOptions, TOptionsBase>(
            string? name = default)
            where TOptions : class, TOptionsBase
            where TOptionsBase : class
        {
            services = services
                .ConfigureInherit<TOptions, TOptionsBase>(name)
                .PostConfigureInherit<TOptions, TOptionsBase>(name)
                ;
            services.TryAddEnumerable(ServiceDescriptor.Singleton<
                IOptionsChangeTokenSource<TOptions>,
                InheritedNamedOptionsChangeTokenSource<TOptions, TOptionsBase>
                >(serviceProvider => InheritedNamedOptionsChangeTokenSource
                <TOptions, TOptionsBase>.CreateInstance(serviceProvider, name)
                ));
            return services;
        }

        public IServiceCollection ConfigureInherit<TOptions, TOptionsBase>(
            string? name = default)
            where TOptions : class, TOptionsBase
            where TOptionsBase : class
        {
            if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;
            name ??= Options.DefaultName;
            var serviceDesc = ServiceDescriptor.Transient<
                IConfigureOptions<TOptions>,
                InheritedConfigureNamedOptions<TOptions, TOptionsBase>
                >(serviceProvider => InheritedConfigureNamedOptions
                <TOptions, TOptionsBase>.CreateInstance(serviceProvider, name)
                );
            services.TryAddEnumerable(serviceDesc);

        returnFluent:
            return services;
        }

        public IServiceCollection PostConfigureInherit<TOptions, TOptionsBase>(
            string? name = default
            )
            where TOptions : class, TOptionsBase
            where TOptionsBase : class
        {
            if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;
            name ??= Options.DefaultName;
            var serviceDesc = ServiceDescriptor.Transient<
                IPostConfigureOptions<TOptions>,
                InheritedPostConfigureNamedOptions<TOptions, TOptionsBase>
                >(serviceProvider => InheritedPostConfigureNamedOptions
                <TOptions, TOptionsBase>.CreateInstance(serviceProvider, name)
                );
            services.TryAddEnumerable(serviceDesc);

        returnFluent:
            return services;
        }

        public IServiceCollection InheritAll<TOptions, TOptionsBase>()
            where TOptions : class, TOptionsBase
            where TOptionsBase : class
        {
            services = services
                .ConfigureInheritAll<TOptions, TOptionsBase>()
                .PostConfigureInheritAll<TOptions, TOptionsBase>()
                ;
            services.TryAddEnumerable(ServiceDescriptor.Singleton<
                IOptionsChangeTokenSource<TOptions>,
                InheritedAllOptionsChangeTokenSource<TOptions, TOptionsBase>
                >());
            return services;
        }

        public IServiceCollection ConfigureInheritAll<TOptions, TOptionsBase>()
            where TOptions : class, TOptionsBase
            where TOptionsBase : class
        {
            if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;

            services.TryAddEnumerable(ServiceDescriptor.Transient<
                IConfigureOptions<TOptions>,
                InheritedConfigureAllOptions<TOptions, TOptionsBase>
                >());

        returnFluent:
            return services;
        }

        public IServiceCollection PostConfigureInheritAll<TOptions, TOptionsBase>()
            where TOptions : class, TOptionsBase
            where TOptionsBase : class
        {
            if (typeof(TOptions) == typeof(TOptionsBase)) goto returnFluent;

            services.TryAddEnumerable(ServiceDescriptor.Transient<
                IPostConfigureOptions<TOptions>,
                InheritedPostConfigureAllOptions<TOptions, TOptionsBase>
                >());

        returnFluent:
            return services;
        }

        public IServiceCollection ConfigureAll<TOptions>(
            Action<string?, TOptions> configureOptions
            )
            where TOptions : class
        {
            services.AddSingleton<IConfigureOptions<TOptions>>(serviceProvider
                => new ConfigureAllOptions<TOptions>(configureOptions));

            return services;
        }

        public IServiceCollection ConfigureAll<TOptions, TDep>(
            Action<string?, TOptions, TDep> configureOptions
            )
            where TOptions : class
            where TDep : class
        {
            services.AddSingleton<IConfigureOptions<TOptions>>(
                serviceProvider => new ConfigureAllOptions<TOptions, TDep>(
                    serviceProvider.GetRequiredService<TDep>(),
                    configureOptions
                    ));

            return services;
        }

        public IServiceCollection PostConfigureAll<TOptions>(
            Action<string?, TOptions> configureOptions
            )
            where TOptions : class
        {
            services.AddSingleton<IPostConfigureOptions<TOptions>>(
                serviceProvider => new PostConfigureAllOptions<TOptions>(
                    configureOptions
                    ));

            return services;
        }

        public IServiceCollection PostConfigureAll<TOptions, TDep>(
            Action<string?, TOptions, TDep> configureOptions
            )
            where TOptions : class
            where TDep : class
        {
            services.AddSingleton<IPostConfigureOptions<TOptions>>(
                serviceProvider => new PostConfigureAllOptions<TOptions, TDep>(
                    serviceProvider.GetRequiredService<TDep>(),
                    configureOptions
                    ));

            return services;
        }
    }


}
