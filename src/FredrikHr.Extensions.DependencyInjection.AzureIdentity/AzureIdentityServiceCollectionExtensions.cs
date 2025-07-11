using Azure.Core.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Azure.Identity;

public static class AzureIdentityServiceCollectionExtensions
{
    public static IServiceCollection AddAzureIdentity(
        this IServiceCollection services
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddOptions();
        services.AddLogging(logging => logging.ForwardAzureEventSource());
        services.AddHttpClient();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IConfigureOptions<TokenCredentialOptions>,
                HttpClientTransportAzureTokenCredentialConfigureOptions<TokenCredentialOptions>
                >()
            );

        services.TryAddSingleton<AzureTokenCredentialFactory>();
        services.TryAddSingleton<
            IOptionsFactory<DefaultAzureCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<AzureCliCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<AzureDeveloperCliCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<AzurePowerShellCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<DeviceCodeCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<EnvironmentCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<InteractiveBrowserCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<SharedTokenCacheCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<VisualStudioCodeCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<VisualStudioCredential>
            >(SingletonTokenCredentialFactory);
        services.TryAddSingleton<
            IOptionsFactory<WorkloadIdentityCredential>
            >(SingletonTokenCredentialFactory);

        services.TryAddSingleton<ManagedIdentityRegistry>();
        services.TryAddTransient <
            IOptionsFactory<ManagedIdentityCredentialOptions>,
            ManagedIdentityCredentialOptionsFactory
            >();

        return services;

        static AzureTokenCredentialFactory SingletonTokenCredentialFactory(
            IServiceProvider serviceProvider
            ) => serviceProvider.GetRequiredService<AzureTokenCredentialFactory>();
    }
}
