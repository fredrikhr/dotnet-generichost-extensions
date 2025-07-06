using System.ServiceModel;
using System.ServiceModel.Description;

using Microsoft.PowerPlatform.Dataverse.Client;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

internal sealed class ServiceClientChannelFactory(
    Func<HttpClientHandler, HttpMessageHandler> httpFactory
    ) : ChannelFactory<IOrganizationServiceAsync2>(
        GetServiceEndpoint(httpFactory)
    )
{
    private static ServiceEndpoint GetServiceEndpoint(
        Func<HttpClientHandler, HttpMessageHandler> httpFactory
        )
    {
        var binding = GetBinding(null!, TimeSpan.FromMinutes(1));
        ServiceEndpoint endpoint = new(
            ContractDescription.GetContract(typeof(IOrganizationServiceAsync2)),
            binding,
            new(default(string)!)
            );
        foreach (var opDesc in endpoint.Contract.Operations)
        {
            try
            {
                if (opDesc.OperationBehaviors[typeof(DataContractSerializerOperationBehavior)]
                    is DataContractSerializerOperationBehavior serializerOpBehavior
                    )
                {
                    serializerOpBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            catch (KeyNotFoundException) { }
        }
        endpoint.EndpointBehaviors.Add(
            new HttpMessageHandlerFactoryEndpointBehavior(httpFactory)
            );
        return endpoint;
    }

    private static BasicHttpBinding GetBinding(Uri serviceUrl, TimeSpan timeout)
    {
        return new(
            (serviceUrl.Scheme == "https")
            ? BasicHttpSecurityMode.Transport
            : BasicHttpSecurityMode.TransportCredentialOnly
            )
        {
            MaxReceivedMessageSize = 2147483647L,
            MaxBufferSize = int.MaxValue,
            SendTimeout = timeout,
            ReceiveTimeout = timeout,
            OpenTimeout = timeout,
            ReaderQuotas =
            {
                MaxStringContentLength = int.MaxValue,
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue
            }
        };
    }
}
