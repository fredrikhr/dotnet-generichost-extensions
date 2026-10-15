using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.TUnit;

namespace Microsoft.Extensions.Logging.TUnit;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block"
    )]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1708: Identifiers should differ by more than case",
    Justification = "extension block"
    )]
public static class TUnitLoggingDependencyInjection
{
    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder AddTUnit()
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

    extension(IServiceCollection services)
    {
        public IServiceCollection UseTUnitTestContextLifetime()
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(services);
#else
            _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

            services.AddSingleton<IHostLifetime, TUnitTestContextLifetime>();

            return services;
        }
    }
}
