using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public sealed class PublicClientApplicationFactory(
    IServiceProvider serviceProvider
    ) : IOptionsFactory<IPublicClientApplication>
{
    private readonly PublicClientApplicationManager _instanceManager =
        serviceProvider.GetRequiredService<PublicClientApplicationManager>();

    public IPublicClientApplication Create(string name)
    {
        IPublicClientApplication clientApplication =
            _instanceManager.Get(name);
        PublicClientApplicationHosted hostedClientApplication = new(
            name,
            serviceProvider,
            clientApplication
            );
        return hostedClientApplication;
    }
}