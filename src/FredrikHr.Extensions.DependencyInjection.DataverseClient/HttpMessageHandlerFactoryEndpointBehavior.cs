using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

internal sealed class HttpMessageHandlerFactoryEndpointBehavior(
    Func<HttpClientHandler, HttpMessageHandler> httpFactory
    )
    : IEndpointBehavior
{
    public void AddBindingParameters(
        ServiceEndpoint endpoint,
        BindingParameterCollection bindingParameters
        )
    {
        bindingParameters.Add(httpFactory);
    }

    void IEndpointBehavior.ApplyClientBehavior(
        ServiceEndpoint endpoint,
        ClientRuntime clientRuntime
        )
    { }

    void IEndpointBehavior.ApplyDispatchBehavior(
        ServiceEndpoint endpoint,
        EndpointDispatcher endpointDispatcher)
    { }

    void IEndpointBehavior.Validate(ServiceEndpoint endpoint) { }
}