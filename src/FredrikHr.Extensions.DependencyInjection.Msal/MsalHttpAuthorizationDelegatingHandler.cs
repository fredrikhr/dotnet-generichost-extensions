namespace Microsoft.Identity.Client;

public class MsalHttpAuthorizationDelegatingHandler
: DelegatingHandler
{
    private bool ShouldHandleRequest(
        HttpRequestMessage request,
        IDictionary<string, object?>? options
        )
    {
        if ((options?.TryGetValue(typeof(MsalHttpClientFactoryHttpHandler).FullName ?? nameof(MsalHttpClientFactoryHttpHandler), out object? msalHttpClientFactoryHandler) ?? false) &&
            msalHttpClientFactoryHandler is MsalHttpClientFactoryHttpHandler
            )
            return false;

        return false;
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
        bool shouldHandle = ShouldHandleRequest(request, options);
        if (shouldHandle)
        {

        }

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

        bool shouldHandle = ShouldHandleRequest(request, options);
        if (shouldHandle)
        {

        }

        return base.Send(request, cancellationToken);
    }
#endif
}