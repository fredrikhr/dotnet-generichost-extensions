namespace Microsoft.Identity.Client;

public class MsalHttpClientFactory(
    IHttpClientFactory httpFactory,
    string? name = default)
    : IMsalHttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        return name switch
        {
            null => httpFactory.CreateClient(),
            _ => httpFactory.CreateClient(name),
        };
    }
}
