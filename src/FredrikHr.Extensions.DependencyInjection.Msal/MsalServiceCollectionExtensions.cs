using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.AppConfig;

namespace Microsoft.Identity.Client;

public static class MsalServiceCollectionExtensions
{
    public static MsalClientServiceCollectionBuilder AddMsal(
        this IServiceCollection services
        ) => new(services);

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> UseManagedIdentityId(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> builder,
        ManagedIdentityId managedIdentityId
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder);
#else
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
#endif
        builder.Services.Configure<ManagedIdentityApplicationOptions>(
            builder.Name,
            options => options.ManagedIdentityId = managedIdentityId
            );
        return builder;
    }
}