using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class PublicClientApplicationBuilderFactory(
    IOptionsMonitor<PublicClientApplicationOptions> optionsProvider,
    IEnumerable<IConfigureOptions<PublicClientApplicationBuilder>> setups,
    IEnumerable<IPostConfigureOptions<PublicClientApplicationBuilder>> postConfigures
    ) : AbstractApplicationBuilderFactory<
        PublicClientApplicationBuilder,
        PublicClientApplicationOptions
        >(optionsProvider, setups, postConfigures)
{
    protected override PublicClientApplicationBuilder CreateInstance(
        PublicClientApplicationOptions options
        ) => PublicClientApplicationBuilder
            .CreateWithApplicationOptions(options);
}
    