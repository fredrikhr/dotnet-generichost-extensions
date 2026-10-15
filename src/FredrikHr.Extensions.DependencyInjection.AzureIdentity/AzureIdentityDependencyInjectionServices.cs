using Azure.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Azure.Identity;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class AzureIdentityDependencyInjectionServices
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAzureIdentity()
        {
            ArgumentNullException.ThrowIfNull(services);

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