using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Microsoft.Identity.Client;

public static class MsalServiceCollectionExtensions
{
    public static IServiceCollection
        AddMsalConfidentialClient(
        this IServiceCollection services,
        Action<OptionsBuilder<ConfidentialClientApplicationOptions>>? configureOptions = default,
        Action<OptionsBuilder<ConfidentialClientApplicationBuilder>>? configureBuilder = default,
        Action<OptionsBuilder<IConfidentialClientApplication>>? configureApplication = default,
        string? name = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<
            IOptionsFactory<ConfidentialClientApplicationBuilder>,
            ConfidentialClientApplicationBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<IConfidentialClientApplication>,
            ConfidentialClientApplicationFactory
            >();

        var optionsBuilder = services
            .AddOptions<ConfidentialClientApplicationOptions>(name);
        configureOptions?.Invoke(optionsBuilder);
        var builderBuilder = services
            .AddOptions<ConfidentialClientApplicationBuilder>(name);
        configureBuilder?.Invoke(builderBuilder);
        var applicationBuilder = services
            .AddOptions<IConfidentialClientApplication>(name);
        configureApplication?.Invoke(applicationBuilder);

        return services;
    }

    public static IServiceCollection
        AddMsalPublicClient(
        this IServiceCollection services,
        Action<OptionsBuilder<PublicClientApplicationOptions>>? addOptionsAction = default,
        Action<OptionsBuilder<PublicClientApplicationBuilder>>? addBuilderAction = default,
        Action<OptionsBuilder<IPublicClientApplication>>? addApplicationAction = default,
        string? name = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<
            IOptionsFactory<PublicClientApplicationBuilder>,
            PublicClientApplicationBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<IPublicClientApplication>,
            PublicClientApplicationFactory
            >();

        var optionsOptsBuilder = services
            .AddOptions<PublicClientApplicationOptions>(name);
        addOptionsAction?.Invoke(optionsOptsBuilder);
        var builderOptsBuilder = services
            .AddOptions<PublicClientApplicationBuilder>(name);
        addBuilderAction?.Invoke(builderOptsBuilder);
        var applicationOptsBuilder = services
            .AddOptions<IPublicClientApplication>(name);
        addApplicationAction?.Invoke(applicationOptsBuilder);

        return services;
    }

    public static IServiceCollection
        AddMsalManagedIdentityClient(
        this IServiceCollection services,
        AppConfig.ManagedIdentityId? managedIdentityId,
        Action<OptionsBuilder<ManagedIdentityApplicationBuilder>>? addBuilderAction = default,
        Action<OptionsBuilder<IManagedIdentityApplication>>? addApplicationAction = default,
        string? name = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton(
            managedIdentityId ?? AppConfig.ManagedIdentityId.SystemAssigned
            );
        services.AddSingleton<
            IOptionsFactory<ManagedIdentityApplicationBuilder>,
            ManagedIdentityApplicationBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<IManagedIdentityApplication>,
            ManagedIdentityApplicationFactory
            >();

        var builderOptsBuilder = services
            .AddOptions<ManagedIdentityApplicationBuilder>(name);
        addBuilderAction?.Invoke(builderOptsBuilder);
        var applicationOptsBuilder = services
            .AddOptions<IManagedIdentityApplication>(name);
        addApplicationAction?.Invoke(applicationOptsBuilder);

        return services;
    }
}