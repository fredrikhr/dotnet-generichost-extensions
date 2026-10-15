using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public sealed class ManagedIdentityApplicationFactory(
    IServiceProvider serviceProvider
    ) : IOptionsFactory<IManagedIdentityApplication>
{
    private readonly ManagedIdentityApplicationManager _instanceManager =
        serviceProvider.GetRequiredService<ManagedIdentityApplicationManager>();

    public IManagedIdentityApplication Create(string name)
    {
        IManagedIdentityApplication clientApplication =
            _instanceManager.Get(name);
        ManagedIdentityApplicationHosted hostedClientApplication = new(
            name,
            serviceProvider,
            clientApplication
            );
        return hostedClientApplication;
    }
}