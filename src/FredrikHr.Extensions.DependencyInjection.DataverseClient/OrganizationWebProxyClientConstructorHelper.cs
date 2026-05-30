using System.Reflection;
using System.ServiceModel;

using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

internal static class OrganizationWebProxyClientConstructorHelper
{
    private const string ClientTypeFqn =
        "Microsoft.PowerPlatform.Dataverse.Client.Connector.OrganizationWebProxyClientAsync";

    internal static IOrganizationServiceAsync CreateWebProxyClient(
        string name,
        OrganizationServiceClientOptions options,
        IHttpMessageHandlerFactory? httpHandlerFactory = null
        )
    {
        Uri serviceUrl = new(options.ServiceUrl);
        object[] ctorArgs = (options.Timeout, options.StrongTypesAssembly) switch
        {
            (null, null) => [serviceUrl, options.UseStrongTypes],
            (TimeSpan timeout, null) => [serviceUrl, timeout, options.UseStrongTypes],
            (null, Assembly strongTypesAssembly) => [serviceUrl, strongTypesAssembly],
            (TimeSpan timeout, Assembly strongTypesAssembly) => [serviceUrl, timeout, strongTypesAssembly],
        };
        object client = typeof(IOrganizationServiceAsync).Assembly.CreateInstance(
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
        var orgSvcInterface = (IOrganizationServiceAsync)client;
        var clientBase = (ClientBase<IOrganizationServiceAsync>)client;
        if (httpHandlerFactory is not null)
        {
            string handlerName = options.HttpClientHandlerName ?? name;
            HttpMessageHandlerFactoryEndpointBehavior httpBehavior = new(
                _ => httpHandlerFactory.CreateHandler(handlerName)
                );
            clientBase.Endpoint.EndpointBehaviors.Add(httpBehavior);
        }
        return orgSvcInterface;
    }
}
