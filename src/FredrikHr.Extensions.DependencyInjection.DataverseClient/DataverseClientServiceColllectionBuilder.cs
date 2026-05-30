using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

public class DataverseClientServiceColllectionBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services
        ?? throw new ArgumentNullException(nameof(services));

    public DataverseClientServiceColllectionBuilder ConfigureAllOrganizationOptions(
        Action<string?, OrganizationServiceClientOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public DataverseClientServiceColllectionBuilder PostConfigureAllOrganizationOptions(
        Action<string?, OrganizationServiceClientOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public DataverseClientServiceColllectionBuilder ConfigureAllClients(
        Action<string?, ServiceClient> configureClient
        )
    {
        Services.ConfigureAll(configureClient);
        return this;
    }

    public DataverseClientServiceColllectionBuilder PostConfigureAllClients(
        Action<string?, ServiceClient> configureClient
        )
    {
        Services.PostConfigureAll(configureClient);
        return this;
    }
}