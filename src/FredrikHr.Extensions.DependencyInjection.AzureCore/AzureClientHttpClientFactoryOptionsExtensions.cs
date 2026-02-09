using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Azure.Core;

public static class AzureClientHttpClientFactoryOptionsExtensions
{
    public static IServiceCollection ConfigureAllUseHttpClientFactory<TOptions>(
        this IServiceCollection services
        )
        where TOptions : ClientOptions
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IConfigureOptions<TOptions>,
                AzureClientHttpClientTransportConfigureOptions<TOptions>
                >()
            );

        return services;
    }
}