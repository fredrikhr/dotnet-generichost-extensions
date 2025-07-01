using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ManagedIdentityApplicationBuilderFactory(
    IServiceProvider serviceProvider,
    IOptionsMonitor<ManagedIdentityApplicationOptions> optionsProvider,
    IEnumerable<IConfigureOptions<ManagedIdentityApplicationBuilder>> setups,
    IEnumerable<IPostConfigureOptions<ManagedIdentityApplicationBuilder>> postConfigures
    ) : OptionsFactory<ManagedIdentityApplicationBuilder>(setups, postConfigures)
{
    protected override ManagedIdentityApplicationBuilder CreateInstance(string name)
    {
        AppConfig.ManagedIdentityId managedIdentityId =
            optionsProvider.Get(name).ManagedIdentityId
            ?? serviceProvider.GetService<AppConfig.ManagedIdentityId>()
            ?? AppConfig.ManagedIdentityId.SystemAssigned;
        return ManagedIdentityApplicationBuilder.Create(managedIdentityId);
    }
}
