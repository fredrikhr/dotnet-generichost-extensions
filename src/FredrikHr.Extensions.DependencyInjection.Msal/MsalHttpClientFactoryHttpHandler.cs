namespace Microsoft.Identity.Client;

internal sealed class MsalHttpClientFactoryHttpHandler(
    HttpMessageHandler httpMessageHandler
    ) : DelegatingHandler(httpMessageHandler)
{
    private void AddOptions(IDictionary<string, object?>? options)
    {
        if (options is null) return;
        options[GetType().FullName ?? nameof(MsalHttpClientFactoryHttpHandler)] = this;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        _ = request ?? throw new ArgumentNullException(nameof(request));
#endif

        var options = request
#if NET5_0_OR_GREATER
            .Options
#else
            .Properties
#endif
            ;

        AddOptions(options);
        return base.SendAsync(request, cancellationToken);
    }

#if NET5_0_OR_GREATER
    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        _ = request ?? throw new ArgumentNullException(nameof(request));
#endif

        var options = request
#if NET5_0_OR_GREATER
            .Options
#else
            .Properties
#endif
            ;

        AddOptions(options);

        return base.Send(request, cancellationToken);
    }
#endif
}
