using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed class StorageCreationPropertiesBuilderFactory(
    IOptionsMonitor<StorageCreationParameters> paramsProvider,
    IEnumerable<IConfigureOptions<StorageCreationPropertiesBuilder>> setups,
    IEnumerable<IPostConfigureOptions<StorageCreationPropertiesBuilder>> postConfigures
    ) : OptionsFactory<StorageCreationPropertiesBuilder>(setups, postConfigures)
{
    protected override StorageCreationPropertiesBuilder CreateInstance(string name)
    {
        var paramInstance = paramsProvider.Get(name);
        return new StorageCreationPropertiesBuilder(
            paramInstance.CacheName, paramInstance.CacheDirectory
            );
    }
}
