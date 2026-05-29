using Azure.Core;

using Microsoft.Extensions.Options;

namespace Azure.Identity;

public class ManagedIdentityCredentialOptionsFactory(
    IOptionsMonitor<ManagedIdentityIdOptions> idProvider,
    IEnumerable<IConfigureOptions<ManagedIdentityCredentialOptions>> setups,
    IEnumerable<IPostConfigureOptions<ManagedIdentityCredentialOptions>> postConfigures,
    IEnumerable<IValidateOptions<ManagedIdentityCredentialOptions>> validations
    ) : OptionsFactory<ManagedIdentityCredentialOptions>(setups, postConfigures, validations)
{
    public ManagedIdentityCredentialOptionsFactory(
        IOptionsMonitor<ManagedIdentityIdOptions> idProvider,
        IEnumerable<IConfigureOptions<ManagedIdentityCredentialOptions>> setups,
        IEnumerable<IPostConfigureOptions<ManagedIdentityCredentialOptions>> postConfigures
    ) : this(idProvider, setups, postConfigures, []) { }

    protected override ManagedIdentityCredentialOptions CreateInstance(string name)
    {
        ManagedIdentityIdOptions idOptions = idProvider.Get(name);
        string? idString = idOptions.Id;
        return new(idOptions.Type switch
        {
            ManagedIdentityIdType.ClientId => ManagedIdentityId
                .FromUserAssignedClientId(idString),
            ManagedIdentityIdType.ResourceId => ManagedIdentityId
                .FromUserAssignedResourceId(ResourceIdentifier.Parse(idString!)),
            ManagedIdentityIdType.ObjectId => ManagedIdentityId
                .FromUserAssignedObjectId(idString),
            _ => ManagedIdentityId.SystemAssigned,
        });
    }
}