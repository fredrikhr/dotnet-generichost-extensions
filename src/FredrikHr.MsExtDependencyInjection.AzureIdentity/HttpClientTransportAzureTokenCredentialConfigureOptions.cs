using Azure.Core.Pipeline;

using Microsoft.Extensions.Options;

namespace Azure.Identity;

internal sealed class HttpClientTransportAzureTokenCredentialConfigureOptions<TOptions>(
        IHttpClientFactory httpFactory
    ) : IConfigureNamedOptions<TOptions> where TOptions : TokenCredentialOptions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000: Dispose objects before losing scope",
        Justification = nameof(HttpClientTransport)
        )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0079: Remove unnecessary suppression"
        )]
    public void Configure(string? name, TOptions options)
    {
        if (options.Transport is IDisposable previousTransport)
            previousTransport.Dispose();
        options.Transport = new HttpClientTransport(name is null
            ? httpFactory.CreateClient()
            : httpFactory.CreateClient(name)
            );
    }

    void IConfigureOptions<TOptions>.Configure(TOptions options) =>
        Configure(Options.DefaultName, options);
}