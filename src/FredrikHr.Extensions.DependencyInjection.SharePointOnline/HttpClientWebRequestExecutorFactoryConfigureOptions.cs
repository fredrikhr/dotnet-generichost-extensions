using Microsoft.Extensions.Options;

namespace Microsoft.SharePoint.Client;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = nameof(Microsoft.Extensions.DependencyInjection)
    )]
internal sealed class HttpClientWebRequestExecutorFactoryConfigureOptions(
    IHttpClientFactory httpClientFactory
    ) : IConfigureNamedOptions<ClientContext>
{
    public void Configure(ClientContext options) =>
        Configure(default, options);

    public void Configure(string? name, ClientContext options)
    {
        ClientContext context = options;
        context.WebRequestExecutorFactory =
            new HttpClientWebRequestExecutorFactory(name, httpClientFactory);
    }
}

