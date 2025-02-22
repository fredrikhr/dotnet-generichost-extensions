using Azure.Core;
using Azure.Core.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Identity;

public static class AzureIdentityServiceCollectionExtensions
{
    public static OptionsBuilder<TOptions> AddAzureTokenCredential<TCredential, TOptions>(
        this IServiceCollection services,
        string? name = null
        )
        where TCredential : TokenCredential
        where TOptions : TokenCredentialOptions
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddHttpClient();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            ILoggerProvider,
            AzureEventSourceLoggingForwarder
            >());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IConfigureOptions<TOptions>,
            HttpClientTransportAzureTokenCredentialConfigureOptions<TOptions>
            >());

        services.AddTransient(serviceProvider =>
        {
            var optsSource = serviceProvider.GetRequiredService
                <IOptionsMonitor<TOptions>>();
            return ActivatorUtilities.CreateInstance<TCredential>(
                serviceProvider,
                optsSource.Get(name)
                );
        });
        if (typeof(TCredential) != typeof(TokenCredential))
        {
            services.AddTransient<TokenCredential>(static serviceProvider =>
                serviceProvider.GetRequiredService<TCredential>()
                );
        }
        return new(services, name);
    }
}