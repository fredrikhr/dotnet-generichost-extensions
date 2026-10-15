namespace Microsoft.SharePoint.Client;

public class HttpClientWebRequestExecutorFactory(
    string? name,
    IHttpClientFactory httpFactory)
    : WebRequestExecutorFactory()
{
    public override WebRequestExecutor CreateWebRequestExecutor(
        ClientRuntimeContext context,
        string requestUrl)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new HttpClientWebRequestExecutor(
            name is not null
            ? httpFactory.CreateClient(name)
            : httpFactory.CreateClient(),
            requestUrl
            );
    }
}
