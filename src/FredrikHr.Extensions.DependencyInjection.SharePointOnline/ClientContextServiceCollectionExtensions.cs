using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.SharePoint.Client;

public static class ClientContextServiceCollectionExtensions
{
    public static IServiceCollection AddClientContextFactory(
        this IServiceCollection services,
        bool useHttpClientFactory = true
        )
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

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