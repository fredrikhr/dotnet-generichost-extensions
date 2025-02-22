using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.TUnit;

namespace Microsoft.Extensions.Logging.TUnit;

public static class TUnitServiceCollectionExtensions
{
    public static IServiceCollection UseTUnitTestContextLifetime(this IServiceCollection services)
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