namespace Microsoft.Identity.Client;

internal class ManagedIdentityApplicationHosted(
    string? name,
    IServiceProvider serviceProvider,
    IManagedIdentityApplication clientApplication
    ) : ApplicationBaseHosted<IManagedIdentityApplication>(
        name,
        serviceProvider,
        clientApplication
        ), IManagedIdentityApplication
{
    public AcquireTokenForManagedIdentityParameterBuilder AcquireTokenForManagedIdentity(
        string resource
        ) => ClientApplication.AcquireTokenForManagedIdentity(resource)
        .UseConfiguredOptions(ServiceProvider, Name);
}