using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Azure.Core.Diagnostics;

public static class AzureEventSourceLoggingForwardingExtensions
{
    public static ILoggingBuilder ForwardAzureEventSource(
        this ILoggingBuilder builder
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder);
#else
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
#endif

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            ILoggerProvider,
            AzureEventSourceLoggingForwarder
            >());

        return builder;
    }
}