using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class MsalDependencyInjection
{
    extension(IServiceCollection services)
    {
        public MsalClientServiceCollectionBuilder AddMsal()
        {
            ArgumentNullException.ThrowIfNull(services);

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
            services.TryAddTransient<
                IOptionsFactory<IPublicClientApplication>,
                PublicClientApplicationFactory
                >();
            services.AddSingleton<PublicClientApplicationManager>();

            services.InheritAll<
                ConfidentialClientApplicationOptions,
                ApplicationOptions
                >();
            services.TryAddTransient<
                IOptionsFactory<ConfidentialClientApplicationBuilder>,
                ConfidentialClientApplicationBuilderFactory
                >();
            services.TryAddTransient<
                IOptionsFactory<IConfidentialClientApplication>,
                ConfidentialClientApplicationFactory
                >();
            services.AddSingleton<ConfidentialClientApplicationManager>();

            services.TryAddSingleton(AppConfig.ManagedIdentityId.SystemAssigned);
            services.TryAddTransient<
                IOptionsFactory<ManagedIdentityApplicationBuilder>,
                ManagedIdentityApplicationBuilderFactory
                >();
            services.TryAddTransient<
                IOptionsFactory<IManagedIdentityApplication>,
                ManagedIdentityApplicationFactory
                >();
            services.AddSingleton<ManagedIdentityApplicationManager>();

            return new(services);
        }
    }
}