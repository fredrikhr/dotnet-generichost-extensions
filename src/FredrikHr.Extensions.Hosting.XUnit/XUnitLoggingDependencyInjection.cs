using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Logging.XUnit;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block"
    )]
public static class XUnitLoggingDependencyInjection
{
    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder AddXUnit()
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
                ILoggerProvider,
                XUnitLoggerProvider
                >());

            return builder;
        }
    }
}
