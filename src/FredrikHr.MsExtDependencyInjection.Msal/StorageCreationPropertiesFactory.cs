using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed class StorageCreationPropertiesFactory(
    IOptionsMonitor<StorageCreationPropertiesBuilder> builderProvider
    ) : OptionsFactory<StorageCreationProperties>([], [])
{
    protected override StorageCreationProperties CreateInstance(string name)
    {
        var builder = builderProvider.Get(name);
        return builder.Build();
    }
}