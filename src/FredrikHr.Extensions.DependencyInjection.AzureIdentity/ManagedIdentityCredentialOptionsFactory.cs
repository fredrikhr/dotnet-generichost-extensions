using Microsoft.Extensions.Options;

namespace Azure.Identity;

public class ManagedIdentityCredentialOptionsFactory(
    ManagedIdentityRegistry managedIdentityRegistry,
    IEnumerable<IConfigureOptions<ManagedIdentityCredentialOptions>> setups,
    IEnumerable<IPostConfigureOptions<ManagedIdentityCredentialOptions>> postConfigures,
    IEnumerable<IValidateOptions<ManagedIdentityCredentialOptions>> validations
    ) : OptionsFactory<ManagedIdentityCredentialOptions>(setups, postConfigures, validations)
{
    public ManagedIdentityCredentialOptionsFactory(
        ManagedIdentityRegistry managedIdentityRegistry,
        IEnumerable<IConfigureOptions<ManagedIdentityCredentialOptions>> setups,
        IEnumerable<IPostConfigureOptions<ManagedIdentityCredentialOptions>> postConfigures
    ) : this(managedIdentityRegistry, setups, postConfigures, []) { }

    protected override ManagedIdentityCredentialOptions CreateInstance(string name)
    {
        var managedIdentityId = managedIdentityRegistry.Get(name);
        return new(managedIdentityId);
    }
}
