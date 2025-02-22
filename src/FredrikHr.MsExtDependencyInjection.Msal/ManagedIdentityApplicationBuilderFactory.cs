using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ManagedIdentityApplicationBuilderFactory(
    AppConfig.ManagedIdentityId managedIdentityId,
    IEnumerable<IConfigureOptions<ManagedIdentityApplicationBuilder>> setups,
    IEnumerable<IPostConfigureOptions<ManagedIdentityApplicationBuilder>> postConfigures
    ) : OptionsFactory<ManagedIdentityApplicationBuilder>(setups, postConfigures)
{
    protected override ManagedIdentityApplicationBuilder CreateInstance(string name)
    {
        return ManagedIdentityApplicationBuilder.Create(managedIdentityId);
    }
}
    