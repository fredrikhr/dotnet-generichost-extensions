using System.ServiceModel;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

public class ServiceClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<OrganizationServiceClientOptions> orgSvcOptionsProvider,
    IEnumerable<IConfigureOptions<ServiceClient>> setups,
    IEnumerable<IPostConfigureOptions<ServiceClient>> postConfigures,
    IEnumerable<IValidateOptions<ServiceClient>> validations,
    ILoggerFactory? loggerFactory = null,
    IHttpMessageHandlerFactory? httpMessageHandlerFactory = null
    ) : OptionsFactory<ServiceClient>(setups, postConfigures, validations)
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory ??
        Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

    protected override ServiceClient CreateInstance(string name)
    {
        OrganizationServiceClientOptions orgSvcOpts = orgSvcOptionsProvider.Get(name);
        string httpClientName = orgSvcOpts.HttpClientHandlerName ?? name;
        IOrganizationServiceAsync orgSvc = OrganizationWebProxyClientConstructorHelper
            .CreateWebProxyClient(
                httpClientName,
                orgSvcOpts,
                httpMessageHandlerFactory
            );
        var orgClient = (ClientBase<IOrganizationServiceAsync>)orgSvc;
        string baseConnectUrl = orgClient.Endpoint?.Address?.Uri?.ToString()
            ?? orgSvcOpts.ServiceUrl;
        HttpClient httpClient = httpClientFactory.CreateClient(httpClientName);
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