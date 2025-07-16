namespace Microsoft.Identity.Client;

public class MsalHttpClientFactory(
    IHttpMessageHandlerFactory httpMsgFactory,
    string? name = default
    ) : IMsalHttpClientFactory
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000: Dispose objects before losing scope",
        Justification = nameof(IHttpMessageHandlerFactory)
        )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0079: Remove unnecessary suppression",
        Justification = "erroneously triggered"
        )]
    public HttpClient GetHttpClient()
    {
        HttpMessageHandler httpMsgHandler = name switch
        {
            null => httpMsgFactory.CreateHandler(),
            _ => httpMsgFactory.CreateHandler(name),
        };
        MsalHttpClientFactoryHttpHandler httpHandler = new(httpMsgHandler);
        return new(httpHandler);
    }
}
