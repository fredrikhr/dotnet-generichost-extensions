using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.SharePoint.Client;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class ClientContextDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClientContextFactory(
            bool useHttpClientFactory = true)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOptions();
            services.TryAddSingleton<
                IOptionsFactory<ClientContext>,
                ClientContextFactory
                >();
            services.TryAddSingleton<ClientContextFactory>();
            if (useHttpClientFactory)
            {
                services.AddHttpClient();
                services.TryAddEnumerable(ServiceDescriptor.Singleton<
                    IConfigureOptions<ClientContext>,
                    HttpClientWebRequestExecutorFactoryConfigureOptions
                    >());
            }

            return services;
        }
    }
}