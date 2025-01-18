using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Logging.TUnit;

public static class TUnitLoggingBuilderExtensions
{
    public static ILoggingBuilder AddTUnit(this ILoggingBuilder builder)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder);
#else
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
#endif

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            ILoggerProvider,
            TUnitLoggerProvider
            >());

        return builder;
    }
}