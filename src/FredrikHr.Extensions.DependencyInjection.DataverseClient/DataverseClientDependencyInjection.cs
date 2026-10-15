using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

public static class DataverseClientDependencyInjection
{
    extension (IServiceCollection services)
    {
        public DataverseClientServiceColllectionBuilder AddDataverseClient()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOptions();
            services.AddLogging();
            services.AddHttpClient();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<
                IOptionsChangeTokenSource<ServiceClient>,
                InheritedAllOptionsChangeTokenSource<ServiceClient, OrganizationServiceClientOptions>
                >());
            services.AddTransient<
                IOptionsFactory<ServiceClient>,
                ServiceClientFactory
                >();
            services.AddSingleton<ServiceClientFactory>();

            services.TryAddSingleton<
                IOptions<IOrganizationService>
                >(static sp => sp.GetRequiredService<IOptions<ServiceClient>>());
            services.TryAddSingleton<
                IOptions<IOrganizationServiceAsync>
                >(static sp => sp.GetRequiredService<IOptions<ServiceClient>>());
            services.TryAddSingleton<
                IOptions<IOrganizationServiceAsync2>
                >(static sp => sp.GetRequiredService<IOptions<ServiceClient>>());

            services.TryAddSingleton<
                IOptionsMonitor<IOrganizationService>
                >(static sp => sp.GetRequiredService<IOptionsMonitor<ServiceClient>>());
            services.TryAddSingleton<
                IOptionsMonitor<IOrganizationServiceAsync>
                >(static sp => sp.GetRequiredService<IOptionsMonitor<ServiceClient>>());
            services.TryAddSingleton<
                IOptionsMonitor<IOrganizationServiceAsync2>
                >(static sp => sp.GetRequiredService<IOptionsMonitor<ServiceClient>>());

            services.TryAddScoped<
                IOptionsSnapshot<IOrganizationService>
                >(static sp => sp.GetRequiredService<IOptionsSnapshot<ServiceClient>>());
            services.TryAddScoped<
                IOptionsSnapshot<IOrganizationServiceAsync>
                >(static sp => sp.GetRequiredService<IOptionsSnapshot<ServiceClient>>());
            services.TryAddScoped<
                IOptionsSnapshot<IOrganizationServiceAsync2>
                >(static sp => sp.GetRequiredService<IOptionsSnapshot<ServiceClient>>());

            services.AddHostedService<DataverseClientHostedServiceProvider>();

            return new(services);
        }
    }
}