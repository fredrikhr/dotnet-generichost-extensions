using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public static class MsalServiceCollectionExtensions
{
    public static MsalClientServiceCollectionBuilder AddMsal(
        this IServiceCollection services
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddOptions();
        services.InheritAll<
            ApplicationOptions,
            BaseApplicationOptions
            >();

        services.InheritAll<
            PublicClientApplicationOptions,
            ApplicationOptions
            >();
        services.TryAddTransient<
            IOptionsFactory<PublicClientApplicationBuilder>,
            PublicClientApplicationBuilderFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<PublicClientApplicationBuilder>,
            InheritedAllOptionsChangeTokenSource<PublicClientApplicationBuilder, PublicClientApplicationOptions>
            >());
        services.TryAddTransient<
            IOptionsFactory<IPublicClientApplication>,
            PublicClientApplicationFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<IPublicClientApplication>,
            InheritedAllOptionsChangeTokenSource<IPublicClientApplication, PublicClientApplicationBuilder>
            >());

        services.InheritAll<
            ConfidentialClientApplicationOptions,
            ApplicationOptions
            >();
        services.TryAddTransient<
            IOptionsFactory<ConfidentialClientApplicationBuilder>,
            ConfidentialClientApplicationBuilderFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<ConfidentialClientApplicationBuilder>,
            InheritedAllOptionsChangeTokenSource<ConfidentialClientApplicationBuilder, ConfidentialClientApplicationOptions>
            >());
        services.TryAddTransient<
            IOptionsFactory<IConfidentialClientApplication>,
            ConfidentialClientApplicationFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<IConfidentialClientApplication>,
            InheritedAllOptionsChangeTokenSource<IConfidentialClientApplication, ConfidentialClientApplicationBuilder>
            >());

        services.TryAddSingleton(AppConfig.ManagedIdentityId.SystemAssigned);
        services.TryAddTransient<
            IOptionsFactory<ManagedIdentityApplicationBuilder>,
            ManagedIdentityApplicationBuilderFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<ManagedIdentityApplicationBuilder>,
            InheritedAllOptionsChangeTokenSource<ManagedIdentityApplicationBuilder, ManagedIdentityApplicationOptions>
            >());
        services.TryAddTransient<
            IOptionsFactory<IManagedIdentityApplication>,
            ManagedIdentityApplicationFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<IManagedIdentityApplication>,
            InheritedAllOptionsChangeTokenSource<IManagedIdentityApplication, ManagedIdentityApplicationBuilder>
            >());

        return new(services);
    }
}