using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ManagedIdentityApplicationFactory(
    IOptionsMonitor<ManagedIdentityApplicationBuilder> builderProvider,
    IEnumerable<IConfigureOptions<IManagedIdentityApplication>> setups,
    IEnumerable<IPostConfigureOptions<IManagedIdentityApplication>> postConfigures,
    IEnumerable<IValidateOptions<IManagedIdentityApplication>> validations
    ) : OptionsFactory<IManagedIdentityApplication>(
        setups, postConfigures, validations
        )
{
    protected override IManagedIdentityApplication CreateInstance(string name)
    {
        var builder = builderProvider.Get(name);
        return builder.Build();
    }
}