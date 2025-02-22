using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public static class MsalServiceCollectionExtensions
{
    public static OptionsBuilder<ConfidentialClientApplicationBuilder>
        AddMsalConfidentialClient(
        this IServiceCollection services,
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
            .AddOptions<ConfidentialClientApplicationBuilder>(name);
        return optionsBuilder;
    }

    public static OptionsBuilder<PublicClientApplicationBuilder>
        AddMsalPublicClient(
        this IServiceCollection services,
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

        var optionsBuilder = services
            .AddOptions<PublicClientApplicationBuilder>(name);
        return optionsBuilder;
    }

    public static OptionsBuilder<ManagedIdentityApplicationBuilder>
        AddMsalManagedIdentityClient(
        this IServiceCollection services,
        AppConfig.ManagedIdentityId? managedIdentityId = default,
        string? name = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.TryAddSingleton(
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

        var optionsBuilder = services
            .AddOptions<ManagedIdentityApplicationBuilder>(name);
        return optionsBuilder;
    }
}