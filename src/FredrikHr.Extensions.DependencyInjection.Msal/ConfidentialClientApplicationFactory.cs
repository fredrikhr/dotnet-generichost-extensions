using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public sealed class ConfidentialClientApplicationFactory(
    IServiceProvider serviceProvider)
    : IOptionsFactory<IConfidentialClientApplication>
{
    private readonly ConfidentialClientApplicationManager _instanceManager =
        serviceProvider.GetRequiredService<ConfidentialClientApplicationManager>();

    public IConfidentialClientApplication Create(string name)
    {
        IConfidentialClientApplication clientApplication =
            _instanceManager.Get(name);
        ConfidentialClientApplicationHosted hostedClientApplication = new(
            name,
            serviceProvider,
            clientApplication
            );
        return hostedClientApplication;
    }
}
