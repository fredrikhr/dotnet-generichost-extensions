namespace Microsoft.Identity.Client;

internal sealed class MsalHttpClientFactoryHttpHandler(
    HttpMessageHandler httpMessageHandler
    ) : DelegatingHandler(httpMessageHandler)
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
        )
    {
        request.AddOptionByType(this);
        return base.SendAsync(request, cancellationToken);
    }

#if NET5_0_OR_GREATER
    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
        )
    {
        request.AddOptionByType(this);
        return base.Send(request, cancellationToken);
    }
#endif
}
