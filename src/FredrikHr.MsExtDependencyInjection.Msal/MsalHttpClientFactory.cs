namespace Microsoft.Identity.Client;

public class MsalHttpClientFactory(
    IHttpClientFactory httpFactory,
    string? name = default
    ) : IMsalHttpClientFactory
{
    public HttpClient GetHttpClient() => name switch
    {
        { Length: > 0 } => httpFactory.CreateClient(name),
        _ => httpFactory.CreateClient()
    };
}