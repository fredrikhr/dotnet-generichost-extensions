using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

public static class MsalCacheHelperServiceCollectionExtensions
{
    public static OptionsBuilder<StorageCreationPropertiesBuilder>
        AddMsalStorageCreationProperties(
        this IServiceCollection services,
        string cacheName,
        string cacheDirectory,
        string? name = default
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.Configure<StorageCreationParameters>(name, opt =>
        {
            opt.CacheName = cacheName;
            opt.CacheDirectory = cacheDirectory;
        });
        services.AddSingleton<
            IOptionsFactory<StorageCreationPropertiesBuilder>,
            StorageCreationPropertiesBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<StorageCreationProperties>,
            StorageCreationPropertiesFactory
            >();
        return new(services, name);
    }
}