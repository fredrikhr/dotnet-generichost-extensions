using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Client;

public static class MsalServiceCollectionExtensions
{
    public static MsalClientServiceCollectionBuilder AddMsal(
        this IServiceCollection services
        ) => new(services);
}