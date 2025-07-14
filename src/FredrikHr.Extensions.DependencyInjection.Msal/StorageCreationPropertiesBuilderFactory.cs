using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed class StorageCreationPropertiesBuilderFactory(
    IOptionsMonitor<StorageCreationParameters> paramsProvider,
    IEnumerable<IConfigureOptions<StorageCreationPropertiesBuilder>> setups,
    IEnumerable<IPostConfigureOptions<StorageCreationPropertiesBuilder>> postConfigures,
    IEnumerable<IValidateOptions<StorageCreationPropertiesBuilder>> validations
    ) : OptionsFactory<StorageCreationPropertiesBuilder>(setups, postConfigures, validations)
{
    public StorageCreationPropertiesBuilderFactory(
        IOptionsMonitor<StorageCreationParameters> paramsProvider,
        IEnumerable<IConfigureOptions<StorageCreationPropertiesBuilder>> setups,
        IEnumerable<IPostConfigureOptions<StorageCreationPropertiesBuilder>> postConfigures
        ) : this(paramsProvider, setups, postConfigures, []) { }

    public const string MsalCacheFileExtension = ".bin";
    public const string MsalDefaultCacheName = "MsalCache";
    public readonly string MsalDefaultCacheDirectory = Path.Combine(
        MsalCacheHelper.UserRootDirectory,
        typeof(IClientApplicationBase).Namespace ?? "Microsoft.Identity.Client",
        "Cache"
        );

    protected override StorageCreationPropertiesBuilder CreateInstance(string name)
    {
        var paramInstance = paramsProvider.Get(name);
        string cacheName = paramInstance.CacheName switch
        {
            { Length: > 0 } cn => cn,
            _ => MsalDefaultCacheName,
        };
        if (!Path.HasExtension(cacheName) && !paramInstance.SuppressFileExtension)
        {
            cacheName += MsalCacheFileExtension;
        }
        string cacheDir = paramInstance.CacheDirectory switch
        {
            { Length: > 0 } cd => cd,
            _ => MsalDefaultCacheDirectory,
        };
        return new StorageCreationPropertiesBuilder(cacheName, cacheDir);
    }
}
