using Azure.Core;
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
        services.ConfigureAllUseHttpClientFactory<TokenCredentialOptions>();
        services.TryAddTransient<AzureTokenCredentialFactory>();

        TryAddTokenCredential
            <DefaultAzureCredential, DefaultAzureCredentialOptions>
            (services);
        TryAddTokenCredential
            <AzureCliCredential, AzureCliCredentialOptions>
            (services);
        TryAddTokenCredential
            <AzureDeveloperCliCredential, AzureDeveloperCliCredentialOptions>
            (services);
        TryAddTokenCredential
            <AzurePowerShellCredential, AzurePowerShellCredentialOptions>
            (services);
        TryAddTokenCredential
            <DeviceCodeCredential, DeviceCodeCredentialOptions>
            (services);
        TryAddTokenCredential
            <EnvironmentCredential, EnvironmentCredentialOptions>
            (services);
        TryAddTokenCredential
            <InteractiveBrowserCredential, InteractiveBrowserCredentialOptions>
            (services);
        TryAddTokenCredential
            <SharedTokenCacheCredential, SharedTokenCacheCredentialOptions>
            (services);
        TryAddTokenCredential
            <VisualStudioCodeCredential, VisualStudioCodeCredentialOptions>
            (services);
        TryAddTokenCredential
            <VisualStudioCredential, VisualStudioCredentialOptions>
            (services);
        TryAddTokenCredential
            <WorkloadIdentityCredential, WorkloadIdentityCredentialOptions>
            (services);
        TryAddTokenCredential
            <ManagedIdentityCredential, ManagedIdentityCredentialOptions>
            (services);

        services.TryAddEnumerable(ServiceDescriptor.Transient<
            IOptionsChangeTokenSource<ManagedIdentityCredentialOptions>,
            InheritedAllOptionsChangeTokenSource<ManagedIdentityCredentialOptions, ManagedIdentityIdOptions>
            >());
        services.TryAddTransient<
            IOptionsFactory<ManagedIdentityCredentialOptions>,
            ManagedIdentityCredentialOptionsFactory
            >();

        return services;
    }

    private static void TryAddTokenCredential<T, TOptions>(
        IServiceCollection services
        ) where T : TokenCredential
        where TOptions : TokenCredentialOptions
    {
        services.InheritAll<TOptions, TokenCredentialOptions>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<
            IOptionsChangeTokenSource<T>,
            InheritedAllOptionsChangeTokenSource<T, TOptions>
            >());
        services.TryAdd(ServiceDescriptor.Transient(
            typeof(IOptionsFactory<T>),
            SingletonTokenCredentialFactory
            ));
    }

    private static AzureTokenCredentialFactory SingletonTokenCredentialFactory(
        IServiceProvider serviceProvider
        ) => serviceProvider.GetRequiredService<AzureTokenCredentialFactory>();
}