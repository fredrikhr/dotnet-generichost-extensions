using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1708: Identifiers should differ by more than case",
    Justification = "extension block")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class MsalCacheHelperDependencyInjection
{
    private static void AddMsalCacheHelperServices(
        IServiceCollection services)
    {
        services.AddOptions();
        services.AddLogging();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<StorageCreationPropertiesBuilder>,
            InheritedAllOptionsChangeTokenSource<StorageCreationPropertiesBuilder, StorageCreationParameters>
            >());
        services.TryAddSingleton<
            IOptionsFactory<StorageCreationPropertiesBuilder>,
            StorageCreationPropertiesBuilderFactory
            >();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<StorageCreationProperties>,
            InheritedAllOptionsChangeTokenSource<StorageCreationProperties, StorageCreationPropertiesBuilder>
            >());
        services.TryAddSingleton<
            IOptionsFactory<StorageCreationProperties>,
            StorageCreationPropertiesFactory
            >();
        services.TryAddSingleton<MsalCacheHelperFactory>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IOptionsChangeTokenSource<Task<MsalCacheHelper>>,
            InheritedAllOptionsChangeTokenSource<Task<MsalCacheHelper>, StorageCreationProperties>
            >());
        services.TryAddSingleton<
            IOptionsFactory<Task<MsalCacheHelper>>
            >(static sp => sp.GetRequiredService<MsalCacheHelperFactory>());
        services.TryAddTransient<
            IValidateOptions<MsalCacheHelper>,
            MsalCacheHelperValidateOptions
            >();
    }

    extension(IServiceCollection services)
    {
        public OptionsBuilder<StorageCreationPropertiesBuilder>
        AddMsalPersistentCacheHelper(
            string cacheName,
            string cacheDirectory,
            string? name = default)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Configure<StorageCreationParameters>(name, opt =>
            {
                opt.CacheName = cacheName;
                opt.CacheDirectory = cacheDirectory;
            });
            AddMsalCacheHelperServices(services);
            return new(services, name);
        }

        public IServiceCollection AddMsalPersistentCacheHelper(
            Func<string?, (string cacheName, string cacheDirectory)> cacheConstructorFactory)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (cacheConstructorFactory is not null)
            {
                services.ConfigureAll<StorageCreationParameters>(
                    (name, ctorParams) => (
                    ctorParams.CacheName,
                    ctorParams.CacheDirectory
                    ) = cacheConstructorFactory(name));
            }
            AddMsalCacheHelperServices(services);
            return services;
        }

        public IServiceCollection AddMsalPersistentCacheHelper()
        {
            ArgumentNullException.ThrowIfNull(services);

            AddMsalCacheHelperServices(services);
            return services;
        }
    }

    extension(MsalClientServiceCollectionBuilder msalBuilder)
    {
        public MsalClientServiceCollectionBuilder UseMsalUserTokenCacheHelper()
        {
            ArgumentNullException.ThrowIfNull(msalBuilder);
            AddMsalCacheHelperServices(msalBuilder.Services);
            msalBuilder.Services.PostConfigureAll<
                IClientApplicationBase,
                IOptionsMonitor<Task<MsalCacheHelper>>
                >(static (name, client, cacheProvider) =>
                {
                    MsalCacheHelper cache = cacheProvider.Get(name).GetAwaiter().GetResult();
                    cache.RegisterCache(client.UserTokenCache);
                });
            msalBuilder.Services.PostConfigureInheritAll<
                IPublicClientApplication,
                IClientApplicationBase
                >();
            msalBuilder.Services.PostConfigureInheritAll<
                IConfidentialClientApplication,
                IClientApplicationBase
                >();
            return msalBuilder;
        }
    }
}