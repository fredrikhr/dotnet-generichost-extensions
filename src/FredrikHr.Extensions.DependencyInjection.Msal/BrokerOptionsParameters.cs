namespace Microsoft.Identity.Client;

internal sealed class BrokerOptionsParameters()
{
    public BrokerOptions.OperatingSystems EnabledOn { get; set; }

    public BrokerOptionsParameters(
        BrokerOptions.OperatingSystems enabledOn
    ) : this()
    {
        EnabledOn = enabledOn;
    }
}
