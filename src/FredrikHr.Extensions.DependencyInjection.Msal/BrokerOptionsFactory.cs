using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class BrokerOptionsFactory(
    BrokerOptionsParameters optionsConstructorParameters,
    IEnumerable<IConfigureOptions<BrokerOptions>> setups,
    IEnumerable<IPostConfigureOptions<BrokerOptions>> postConfigures,
    IEnumerable<IValidateOptions<BrokerOptions>> validations
    ) : OptionsFactory<BrokerOptions>(setups, postConfigures, validations)
{
    public BrokerOptionsFactory(
        BrokerOptionsParameters optionsConstructorParameters,
        IEnumerable<IConfigureOptions<BrokerOptions>> setups,
        IEnumerable<IPostConfigureOptions<BrokerOptions>> postConfigures
        ) : this(optionsConstructorParameters, setups, postConfigures, []) { }

    protected override BrokerOptions CreateInstance(string name)
    {
        return new(optionsConstructorParameters.EnabledOn);
    }
}