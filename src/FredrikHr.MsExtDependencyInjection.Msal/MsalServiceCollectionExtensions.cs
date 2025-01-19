using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public static class MsalServiceCollectionExtensions
{
    private static void AddBaseAbstractApplicationBuilder<TApplicationInterface, TApplicationOptions, TBuilder>(
        this OptionsBuilder<TBuilder> optionsBuilder,
        Func<TApplicationOptions, TBuilder> builderFactory,
        Func<IServiceProvider, object> applicationFactory,
        ServiceLifetime serviceLifetime
        )
        where TApplicationInterface : IApplicationBase
        where TApplicationOptions : class
        where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
    {
        optionsBuilder.ConfigureApplyBaseType<TBuilder, BaseAbstractApplicationBuilder<TBuilder>>();
        optionsBuilder.PostConfigureApplyBaseType<TBuilder, BaseAbstractApplicationBuilder<TBuilder>>();

        optionsBuilder.Services.AddOptions();
        optionsBuilder.Services.TryAddTransient(
            ConfigurableServiceDescriptor.Factory(
                sp => builderFactory(sp.GetRequiredService<TApplicationOptions>()),
                optionsBuilder.Name
            ));
        optionsBuilder.Services.TryAdd(ServiceDescriptor.Describe(
            serviceType: typeof(TApplicationInterface),
            implementationFactory: applicationFactory,
            serviceLifetime));
    }

    private static void AddAbstractApplicationBuilder<TApplicationInterface, TApplicationOptions, TBuilder>(
        this OptionsBuilder<TBuilder> optionsBuilder,
        Func<TApplicationOptions, TBuilder> builderFactory,
        Func<IServiceProvider, object> applicationFactory,
        ServiceLifetime serviceLifetime
        )
        where TApplicationInterface : IApplicationBase
        where TApplicationOptions : class
        where TBuilder : AbstractApplicationBuilder<TBuilder>
    {
        optionsBuilder.AddBaseAbstractApplicationBuilder<TApplicationInterface, TApplicationOptions, TBuilder>(
            builderFactory,
            applicationFactory,
            serviceLifetime
            );
        optionsBuilder.ConfigureApplyBaseType<TBuilder, AbstractApplicationBuilder<TBuilder>>();
        optionsBuilder.PostConfigureApplyBaseType<TBuilder, AbstractApplicationBuilder<TBuilder>>();
    }

    public static OptionsBuilder<ConfidentialClientApplicationBuilder>
        AddMsalConfidentialClient(
        this IServiceCollection services,
        string? name = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder =
            new(services, name);
        optionsBuilder.AddAbstractApplicationBuilder<
            IConfidentialClientApplication,
            IOptionsMonitor<ConfidentialClientApplicationOptions>,
            ConfidentialClientApplicationBuilder
            >(
            opts => ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(opts.Get(optionsBuilder.Name)),
            CreateApplicationFactory<IConfidentialClientApplication, ConfidentialClientApplicationBuilder>(
                static b => b.Build(), optionsBuilder.Name
                ),
            serviceLifetime);

        return optionsBuilder;
    }

    public static OptionsBuilder<PublicClientApplicationBuilder>
        AddMsalPublicClient(
        this IServiceCollection services,
        string? name = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif
        OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder =
            new(services, name);
        optionsBuilder.AddAbstractApplicationBuilder<
            IPublicClientApplication,
            IOptionsMonitor<PublicClientApplicationOptions>,
            PublicClientApplicationBuilder
            >(
            opts => PublicClientApplicationBuilder
                .CreateWithApplicationOptions(opts.Get(optionsBuilder.Name)),
            CreateApplicationFactory<IPublicClientApplication, PublicClientApplicationBuilder>(
                static b => b.Build(), optionsBuilder.Name
                ),
            serviceLifetime);

        return optionsBuilder;
    }

    public static OptionsBuilder<ManagedIdentityApplicationBuilder>
        AddMsalManagedIdentityClient(
        this IServiceCollection services,
        string? name = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder =
            new(services, name);
        services.TryAddSingleton(AppConfig.ManagedIdentityId.SystemAssigned);
        optionsBuilder.AddBaseAbstractApplicationBuilder<
            IManagedIdentityApplication,
            AppConfig.ManagedIdentityId,
            ManagedIdentityApplicationBuilder
            >(
            ManagedIdentityApplicationBuilder.Create,
            CreateApplicationFactory<
                IManagedIdentityApplication,
                ManagedIdentityApplicationBuilder
                >(static b => b.Build(), optionsBuilder.Name),
            ServiceLifetime.Singleton);
        return optionsBuilder;
    }

    private static Func<IServiceProvider, TApplication> CreateApplicationFactory<
        TApplication,
        TBuilder
        >(
        Func<TBuilder, TApplication> builderBuildFunction,
        string name
        )
        where TApplication : class, IApplicationBase
        where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
    {
        return ConfigurableServiceDescriptor.Factory(
            sp => builderBuildFunction(sp.GetRequiredService<TBuilder>()),
            name
            );
    }
}
