using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ConfidentialClientApplicationBuilderFactory(
    IOptionsMonitor<ConfidentialClientApplicationOptions> optionsProvider,
    IEnumerable<IConfigureOptions<ConfidentialClientApplicationBuilder>> setups,
    IEnumerable<IPostConfigureOptions<ConfidentialClientApplicationBuilder>> postConfigures
    ) : AbstractApplicationBuilderFactory<
        ConfidentialClientApplicationBuilder,
        ConfidentialClientApplicationOptions
        >(optionsProvider, setups, postConfigures)
{
    protected override ConfidentialClientApplicationBuilder CreateInstance(
        ConfidentialClientApplicationOptions options
        ) => ConfidentialClientApplicationBuilder
            .CreateWithApplicationOptions(options);
}
