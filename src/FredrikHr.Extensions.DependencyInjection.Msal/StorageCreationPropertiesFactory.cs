using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed class StorageCreationPropertiesFactory(
    IOptionsMonitor<StorageCreationPropertiesBuilder> builderProvider
    ) : IOptionsFactory<StorageCreationProperties>
{
    public StorageCreationProperties Create(string name)
    {
        var builder = builderProvider.Get(name);
        return builder.Build();
    }
}
