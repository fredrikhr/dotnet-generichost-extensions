using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

public static class MsalCacheHelperServiceCollectionExtensions
{
    private static void AddMsalCacheHelperServices(
        IServiceCollection services
        )
    {
        services.AddOptions();
        services.AddLogging();
        services.TryAddSingleton<
            IOptionsFactory<StorageCreationPropertiesBuilder>,
            StorageCreationPropertiesBuilderFactory
            >();
        services.TryAddSingleton<
            IOptionsFactory<StorageCreationProperties>,
            StorageCreationPropertiesFactory
            >();
        services.TryAddSingleton<MsalCacheHelperFactory>();
        services.TryAddSingleton<
            IOptionsFactory<Task<MsalCacheHelper>>
            >(static sp => sp.GetRequiredService<MsalCacheHelperFactory>());
        services.TryAddTransient<
            IValidateOptions<MsalCacheHelper>,
            MsalCacheHelperValidateOptions
            >();
    }

    public static OptionsBuilder<StorageCreationPropertiesBuilder>
        AddMsalPersistentCacheHelper(
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
        AddMsalCacheHelperServices(services);
        return new(services, name);
    }

    public static IServiceCollection AddMsalPersistentCacheHelper(
        this IServiceCollection services,
        Func<string?, (string cacheName, string cacheDirectory)> cacheConstructorFactory
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        if (cacheConstructorFactory is not null)
        {
            services.ConfigureAllNamed<StorageCreationParameters>(
                (name, ctorParams) => (
                ctorParams.CacheName,
                ctorParams.CacheDirectory
                ) = cacheConstructorFactory(name));
        }
        AddMsalCacheHelperServices(services);
        return services;
    }

    public static IServiceCollection AddMsalPersistentCacheHelper(
        this IServiceCollection services
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        AddMsalCacheHelperServices(services);
        return services;
    }

    public static MsalClientServiceCollectionBuilder UseMsalUserTokenCacheHelper(
        this MsalClientServiceCollectionBuilder msalBuilder
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(msalBuilder);
#else
        _ = msalBuilder ?? throw new ArgumentNullException(nameof(msalBuilder));
#endif
        AddMsalCacheHelperServices(msalBuilder.Services);
        msalBuilder.Services.PostConfigureAllNamed<
            IClientApplicationBase,
            IOptionsMonitor<Task<MsalCacheHelper>>
            >(static (name, client, cacheProvider) =>
            {
                var cache = cacheProvider.Get(name).GetAwaiter().GetResult();
                cache.RegisterCache(client.UserTokenCache);
            });
        msalBuilder.Services.PostConfigureAllInherited<
            IPublicClientApplication,
            IClientApplicationBase
            >();
        msalBuilder.Services.PostConfigureAllInherited<
            IConfidentialClientApplication,
            IClientApplicationBase
            >();
        return msalBuilder;
    }
}