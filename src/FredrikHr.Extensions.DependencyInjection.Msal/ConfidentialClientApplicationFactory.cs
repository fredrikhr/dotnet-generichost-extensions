using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ConfidentialClientApplicationFactory(
    IOptionsMonitor<ConfidentialClientApplicationBuilder> builderProvider,
    IEnumerable<IConfigureOptions<IConfidentialClientApplication>> setups,
    IEnumerable<IPostConfigureOptions<IConfidentialClientApplication>> postConfigures,
    IEnumerable<IValidateOptions<IConfidentialClientApplication>> validations
    ) : OptionsFactory<IConfidentialClientApplication>(
        setups, postConfigures, validations
        )
{
    protected override IConfidentialClientApplication CreateInstance(string name)
    {
        ConfidentialClientApplicationBuilder builder = builderProvider.Get(name);
        return builder.Build();
    }
}
