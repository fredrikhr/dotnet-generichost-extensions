using Azure.Core.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Core;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1708: Identifiers should differ by more than case",
    Justification = "extension block")]
public static class AzureCoreDependencyInjectionServices
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureAllUseHttpClientFactory<TOptions>()
            where TOptions : ClientOptions
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IConfigureOptions<TOptions>,
                    AzureClientHttpClientTransportConfigureOptions<TOptions>
                    >()
                );

            return services;
        }
    }

    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder ForwardAzureEventSource()
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
                ILoggerProvider,
                AzureEventSourceLoggingForwarder
                >());

            return builder;
        }
    }
}