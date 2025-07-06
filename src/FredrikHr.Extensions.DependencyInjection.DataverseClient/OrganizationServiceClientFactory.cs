using System.Reflection;
using System.ServiceModel;

using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

public class OrganizationServiceClientFactory(
    IOptionsMonitor<OrganizationServiceClientOptions> optionsProvider,
    IEnumerable<IConfigureOptions<ClientBase<IOrganizationServiceAsync>>> setups,
    IEnumerable<IPostConfigureOptions<ClientBase<IOrganizationServiceAsync>>> postConfigures,
    IEnumerable<IValidateOptions<ClientBase<IOrganizationServiceAsync>>> validations,
    IHttpMessageHandlerFactory? httpHandlerFactory = null
    ) : OptionsFactory<ClientBase<IOrganizationServiceAsync>>(setups, postConfigures, validations),
    IOptionsFactory<IOrganizationServiceAsync>
{
    private const string ClientTypeFqn =
        "Microsoft.PowerPlatform.Dataverse.Client.Connector.OrganizationWebProxyClientAsync";

    protected override ClientBase<IOrganizationServiceAsync> CreateInstance(string name)
    {
        var options = optionsProvider.Get(name);
        Uri serviceUrl = new(options.ServiceUrl);
        object[] ctorArgs = (options.Timeout, options.StrongTypesAssembly) switch
        {
            (null, null) => [serviceUrl, options.UseStrongTypes],
            (TimeSpan timeout, null) => [serviceUrl, timeout, options.UseStrongTypes],
            (null, Assembly strongTypesAssembly) => [serviceUrl, strongTypesAssembly],
            (TimeSpan timeout, Assembly strongTypesAssembly) => [serviceUrl, timeout, strongTypesAssembly],
        };
        var client = typeof(IOrganizationServiceAsync).Assembly.CreateInstance(
            ClientTypeFqn,
            ignoreCase: true,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic,
            Type.DefaultBinder,
            ctorArgs,
            System.Globalization.CultureInfo.InvariantCulture,
            default
            ) ?? throw new InvalidOperationException($"Failed to construct instance of type {ClientTypeFqn}.");
        var clientBase = (ClientBase<IOrganizationServiceAsync>)client!;
        if (httpHandlerFactory is not null)
        {
            string handlerName = options.HttpClientHandlerName ?? name;
            HttpMessageHandlerFactoryEndpointBehavior httpBehavior = new(
                _ => httpHandlerFactory.CreateHandler(handlerName)
                );
            clientBase.Endpoint.EndpointBehaviors.Add(httpBehavior);
        }
        
        return clientBase;
    }

    IOrganizationServiceAsync IOptionsFactory<IOrganizationServiceAsync>.Create(string name)
    {
        return (IOrganizationServiceAsync)Create(name);
    }
}
