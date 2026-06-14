using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal static class BaseAbstractAcquireTokenOptions
{
    extension<TBuilder>(TBuilder acquireTokenBuilder)
        where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        internal TBuilder UseConfiguredOptions(
            IServiceProvider serviceProvider,
            string? name = default
            )
        {
            OptionsFactoryHelper.Apply(acquireTokenBuilder, name, serviceProvider);
            return acquireTokenBuilder;
        }
    }
}
