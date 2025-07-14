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

    protected override StorageCreationPropertiesBuilder CreateInstance(string name)
    {
        var paramInstance = paramsProvider.Get(name);
        return new StorageCreationPropertiesBuilder(
            paramInstance.CacheName, paramInstance.CacheDirectory
            );
    }
}
