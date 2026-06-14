using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ManagedIdentityApplicationBuilderFactory(
    IOptionsMonitor<ManagedIdentityApplicationOptions> optionsProvider,
    IEnumerable<IConfigureOptions<ManagedIdentityApplicationBuilder>> setups,
    IEnumerable<IPostConfigureOptions<ManagedIdentityApplicationBuilder>> postConfigures,
    AppConfig.ManagedIdentityId? managedIdentityId = default
    ) : OptionsFactory<ManagedIdentityApplicationBuilder>(setups, postConfigures)
{
    private AppConfig.ManagedIdentityId ManagedIdentityId { get; } = managedIdentityId
        ?? AppConfig.ManagedIdentityId.SystemAssigned;

    protected override ManagedIdentityApplicationBuilder CreateInstance(string name)
    {
        AppConfig.ManagedIdentityId mii =
            optionsProvider.Get(name).ManagedIdentityId
            ?? ManagedIdentityId;
        return ManagedIdentityApplicationBuilder.Create(mii);
    }
}
