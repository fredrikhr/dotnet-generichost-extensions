using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;

namespace Microsoft.SharePoint.Client;

public class HttpClientWebRequestExecutor : WebRequestExecutor
{
    private readonly CancellationTokenSource _cts = new();
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _requestMessage;
    private readonly HttpClientWebRequestExectuorRequestContent _requestContent;
    private Task<HttpResponseMessage>? _responseTask;
    private HttpResponseMessage? _responseMessage;
    private HttpClientWebHeaderCollection? _responseHeaders;

    public override string? RequestContentType
    {
        get => _requestContent.Headers.ContentType.ToString();
        set => _requestContent.Headers.ContentType = new(value);
    }

    public override WebHeaderCollection RequestHeaders { get; } = [];

    private static readonly Dictionary<string, HttpMethod> KnownHttpMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { HttpMethod.Get.Method, HttpMethod.Get },
            { HttpMethod.Put.Method, HttpMethod.Put },
            { HttpMethod.Post.Method, HttpMethod.Post },
            { HttpMethod.Delete.Method, HttpMethod.Delete },
            { HttpMethod.Head.Method, HttpMethod.Head },
            { HttpMethod.Options.Method, HttpMethod.Options },
            { HttpMethod.Trace.Method, HttpMethod.Trace },
        };

    public override string RequestMethod
    {
        get => _requestMessage.Method.Method;
        set
        {
            if (!KnownHttpMethods.TryGetValue(value, out HttpMethod method))
                method = new(value);
            _requestMessage.Method = method;
        }
    }
    public override bool RequestKeepAlive
    {
        get
        {
            bool? connClose = _requestMessage.Headers.ConnectionClose;
            return !(connClose.HasValue && connClose.Value == false);
        }
        set
        {
            _requestMessage.Headers.ConnectionClose = !value;
        }
    }
    public override HttpStatusCode StatusCode =>
        (_responseMessage ?? throw new InvalidOperationException()).StatusCode;

    public override string? ResponseContentType => _responseMessage?
        .Content.Headers.ContentType?.ToString();
    public override WebHeaderCollection? ResponseHeaders => _responseHeaders;

    [SuppressMessage(
        "Design",
        "CA1054: URI-like parameters should not be strings",
        Justification = nameof(WebRequestExecutorFactory.CreateWebRequestExecutor)
        )]
    public HttpClientWebRequestExecutor(HttpClient httpClient, string requestUrl)
    {
        _ = requestUrl ?? throw new ArgumentNullException(nameof(requestUrl));

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _requestContent = new();
        _requestMessage = new()
        {
            RequestUri = new(requestUrl),
            Method = HttpMethod.Post,
            Content = _requestContent,
        };
    }

    public override void Execute() => ExecuteAsync().GetAwaiter().GetResult();

    public async
#if NETSTANDARD
        override
#endif
        Task ExecuteAsync()
    {
        if (_responseTask is null)
            SendRequest();
        var respMessage = await _responseTask
            .ConfigureAwait(continueOnCapturedContext: false);
        respMessage.EnsureSuccessStatusCode();
        _responseMessage = respMessage;
        _responseHeaders = [];
        _responseHeaders.AddHeaders(respMessage.Headers);
        _responseHeaders.AddHeaders(respMessage.Content.Headers);
    }

    public override Stream GetRequestStream()
    {
        SendRequest();
        return new HttpClientWebRequestExectuorRequestStream(
            _requestContent.StreamCompletionSource.Task.GetAwaiter().GetResult(),
            _requestContent.RequestPayloadCompletionSource
            );
    }

    [MemberNotNull(nameof(_responseTask))]
    private void SendRequest()
    {
        foreach (string name in RequestHeaders)
        {
            HttpHeaders headers = WellKnownContentHeaders.Contains(name)
                ? _requestContent.Headers
                : _requestMessage.Headers;
            headers.TryAddWithoutValidation(name, RequestHeaders[name]);
        }

        if (RequestMethod.Equals(HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase) ||
            RequestMethod.Equals(HttpMethod.Head.Method, StringComparison.OrdinalIgnoreCase)
            )
        {
            _requestContent.StreamCompletionSource.TrySetException(
                new InvalidOperationException()
                );
            _requestMessage.Content = null;
        }
        _responseTask = _httpClient.SendAsync(
            _requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            _cts.Token
            );
    }

    public override Stream GetResponseStream()
    {
        return _responseMessage is null
            ? throw new InvalidOperationException()
            : _responseMessage.Content.ReadAsStreamAsync()
            .GetAwaiter().GetResult();
    }

    private static readonly HashSet<string> WellKnownContentHeaders = new(
        [
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            "Content-Type",
            "Expires",
            "Last-Modified",
        ],
        StringComparer.OrdinalIgnoreCase
        );

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);

        _httpClient.Dispose();
        _requestContent.Dispose();
        _requestMessage.Content = null;
        _requestMessage.Dispose();
        _responseTask?.Dispose();
        if (_responseMessage is IDisposable respMessage)
        {
            _responseMessage = null;
            respMessage.Dispose();
        }
        _cts.Cancel();
        _cts.Dispose();
    }

    private sealed class HttpClientWebHeaderCollection() : WebHeaderCollection()
    {
        public void AddHeaders(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers
            )
        {
            foreach (var namerHeaderLinesPair in headers)
            {
                string name = namerHeaderLinesPair.Key;
                foreach (string value in namerHeaderLinesPair.Value)
                {
                    AddWithoutValidate(name, value);
                }
            }
        }
    }
}
