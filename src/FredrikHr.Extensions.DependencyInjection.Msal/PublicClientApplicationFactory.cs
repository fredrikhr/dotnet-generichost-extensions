using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class PublicClientApplicationFactory(
    IOptionsMonitor<PublicClientApplicationBuilder> builderProvider,
    IEnumerable<IConfigureOptions<IPublicClientApplication>> setups,
    IEnumerable<IPostConfigureOptions<IPublicClientApplication>> postConfigures,
    IEnumerable<IValidateOptions<IPublicClientApplication>> validations
    ) : OptionsFactory<IPublicClientApplication>(
        setups, postConfigures, validations
        )
{
    protected override IPublicClientApplication CreateInstance(string name)
    {
        PublicClientApplicationBuilder builder = builderProvider.Get(name);
        return builder.Build();
    }
}
