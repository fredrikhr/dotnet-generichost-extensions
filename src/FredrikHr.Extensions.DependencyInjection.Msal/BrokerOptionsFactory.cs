using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class BrokerOptionsFactory(
    IOptionsMonitor<BrokerOptionsParameters> optionsParameterProvider,
    IEnumerable<IConfigureOptions<BrokerOptions>> setups,
    IEnumerable<IPostConfigureOptions<BrokerOptions>> postConfigures,
    IEnumerable<IValidateOptions<BrokerOptions>> validations
    ) : OptionsFactory<BrokerOptions>(setups, postConfigures, validations)
{
    public BrokerOptionsFactory(
        IOptionsMonitor<BrokerOptionsParameters> optionsParameterProvider,
        IEnumerable<IConfigureOptions<BrokerOptions>> setups,
        IEnumerable<IPostConfigureOptions<BrokerOptions>> postConfigures
        ) : this(optionsParameterProvider, setups, postConfigures, []) { }

    protected override BrokerOptions CreateInstance(string name)
    {
        var optionParams = optionsParameterProvider.Get(name);
        return new(optionParams.EnabledOn);
    }
}