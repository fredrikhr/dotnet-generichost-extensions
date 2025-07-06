using System.ServiceModel;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

public class ServiceClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<IOrganizationServiceAsync> orgSvcProvider,
    IOptionsMonitor<OrganizationServiceClientOptions> orgSvcOptionsProvider,
    IEnumerable<IConfigureOptions<ServiceClient>> setups,
    IEnumerable<IPostConfigureOptions<ServiceClient>> postConfigures,
    IEnumerable<IValidateOptions<ServiceClient>> validations,
    ILoggerFactory? loggerFactory = null
    ) : OptionsFactory<ServiceClient>(setups, postConfigures, validations)
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory ??
        Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

    protected override ServiceClient CreateInstance(string name)
    {
        var orgSvc = orgSvcProvider.Get(name);
        var orgClient = (ClientBase<IOrganizationServiceAsync>)orgSvc;
        var httpClientName = orgSvcOptionsProvider.Get(name).HttpClientHandlerName ?? name;
        HttpClient httpClient = httpClientFactory.CreateClient(httpClientName);
        string baseConnectUrl = orgClient.Endpoint.Address.Uri.ToString();
        Version? targetVersion = default;
        return ServiceClientConstructorHelper.CreateServiceClient(
            orgSvc,
            httpClient,
            baseConnectUrl,
            targetVersion,
            _loggerFactory.CreateLogger<ServiceClient>()
            );
    }
}