namespace Microsoft.Identity.Client;

internal sealed class BrokerOptionsParameters(
    BrokerOptions.OperatingSystems enabledOn
    )
{
    public BrokerOptions.OperatingSystems EnabledOn { get; } = enabledOn;
}
