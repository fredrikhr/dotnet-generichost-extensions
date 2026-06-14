using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization.Metadata;

using Microsoft.Extensions.Options;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity;

public class TokenAcquirerDownstreamApi(
    IHttpClientFactory httpFactory,
    IAuthorizationHeaderProvider authorizationHeaderProvider,
    IOptionsMonitor<DownstreamApiOptions> downstreamOptionsProvider
    ) : IDownstreamApi
{
    private enum CallApiMode
    {
        Unspecified = default,
        ForUser,
        ForApp,
    }

    private enum ResponseHandlingMode
    {
        Ignore = -1,
        Deserialize = default,
        Message
    }

#if !NET
    private static HttpMethod HttpPatch => field ?? new("PATCH");
#endif

    private readonly IHttpClientFactory _httpFactory = httpFactory
        ?? throw new ArgumentNullException(nameof(httpFactory));
    private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider
        = authorizationHeaderProvider
        ?? throw new ArgumentNullException(nameof(authorizationHeaderProvider));
    private readonly IOptionsMonitor<DownstreamApiOptions> _downstreamOptionsProvider
        = downstreamOptionsProvider
        ?? throw new ArgumentNullException(nameof(downstreamOptionsProvider));

    private HttpClient CreateMtlsHttpClient(X509Certificate2 mtlsCertificate)
    {
        throw new NotImplementedException();
    }

    private async Task<HttpResponseMessage> CallApiAsync(
        HttpMethod? httpMethod,
        DownstreamApiOptions options,
        string? serviceName,
        ClaimsPrincipal? user = null,
        HttpContent? content = null,
        CallApiMode callMode = default,
        CancellationToken cancellationToken = default)
    {
        httpMethod ??= options.HttpMethod is { Length: > 0 } httpMethodString
            ? new(httpMethodString)
            : HttpMethod.Get;
        UriBuilder requUri = new(options.GetApiUrl());
        if (options.ExtraQueryParameters is { Count: > 0 } extraQueries)
        {
            StringBuilder queryBuilder = new(requUri.Query ?? "");
            foreach(KeyValuePair<string, string> extraQuery in extraQueries)
            {
                if (string.IsNullOrEmpty(extraQuery.Key)) continue;
                queryBuilder.Append(queryBuilder.Length > 0 ? "&" : "?");
                queryBuilder.Append(Uri.EscapeDataString(extraQuery.Key));
                queryBuilder.Append('=');
                queryBuilder.Append(Uri.EscapeDataString(extraQuery.Value ?? ""));
            }
            requUri.Query = queryBuilder.ToString();
        }
        using HttpRequestMessage httpRequMsg = new(httpMethod, requUri.Uri);
        if (options.AcceptHeader is { Length: > 0 } acceptHeader)
        {
            httpRequMsg.Headers.Accept.TryParseAdd(acceptHeader);
        }
        if (options.ExtraHeaderParameters is { } extraHeaders)
        {
            foreach (KeyValuePair<string, string> extraHeader in extraHeaders)
            {
                httpRequMsg.Headers.TryAddWithoutValidation(extraHeader.Key, extraHeader.Value);
            }
        }
        if (content is not null)
        {
            if (options.ContentType is { Length: > 0 } contentType)
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }
            httpRequMsg.Content = content;
        }
        X509Certificate2? bindingCertificate = null;
        string? authorizationHeaderString = null;
        const StringComparison ordIgn = StringComparison.OrdinalIgnoreCase;
        if (TokenAcquirerAuthorizationHeaderProvider.TokenBindingProtocolScheme.Equals(options.ProtocolScheme, ordIgn) &&
            _authorizationHeaderProvider is IBoundAuthorizationHeaderProvider boundAuthorizationHeaderProvider)
        {
            var authorizationResult = await boundAuthorizationHeaderProvider
                .CreateBoundAuthorizationHeaderAsync(
                    options,
                    user,
                    cancellationToken
                    ).ConfigureAwait(continueOnCapturedContext: false);
            if (authorizationResult.Succeeded)
            {
                AuthorizationHeaderInformation authorizationInfo
                    = authorizationResult.Result!;
                (authorizationHeaderString, bindingCertificate) = (
                    authorizationInfo.AuthorizationHeaderValue,
                    authorizationInfo.BindingCertificate
                    );
            }
        }
        else
        {
            authorizationHeaderString = callMode switch
            {
                CallApiMode.ForUser => await _authorizationHeaderProvider
                    .CreateAuthorizationHeaderForUserAsync(
                        options.Scopes ?? [],
                        options,
                        user,
                        cancellationToken
                    ).ConfigureAwait(continueOnCapturedContext: false),
                CallApiMode.ForApp => await _authorizationHeaderProvider
                    .CreateAuthorizationHeaderForAppAsync(
                        options.Scopes?.SingleOrDefault()
                        ?? throw new InvalidOperationException(
                            "Unable to create authorization header for downstream API call: No scopes provided."
                            ),
                        options,
                        cancellationToken
                    ).ConfigureAwait(continueOnCapturedContext: false),
                _ => await _authorizationHeaderProvider
                    .CreateAuthorizationHeaderAsync(
                        options.Scopes ?? [],
                        options,
                        user,
                        cancellationToken
                    ).ConfigureAwait(continueOnCapturedContext: false),
            }; 
        }
        if (!string.IsNullOrEmpty(authorizationHeaderString))
        {
            httpRequMsg.Headers.Authorization = AuthenticationHeaderValue.Parse(
                authorizationHeaderString
                );
        }
        options.CustomizeHttpRequestMessage?.Invoke(httpRequMsg);
        using HttpClient httpClient = bindingCertificate is not null
            ? CreateMtlsHttpClient(bindingCertificate)
            : string.IsNullOrEmpty(serviceName)
            ? _httpFactory.CreateClient()
            : _httpFactory.CreateClient(serviceName!);
        return await httpClient.SendAsync(httpRequMsg, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<TOutput?> CallApiAsync<TInput, TOutput>(
        HttpMethod? httpMethod,
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput>? inputTypeInfo = null,
        JsonTypeInfo<TOutput>? outputTypeInfo = null,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CallApiMode callMode = default,
        ResponseHandlingMode responseHandling = default,
        CancellationToken cancellationToken = default) where TOutput : class
    {
        DownstreamApiOptions options =
            _downstreamOptionsProvider.Get(serviceName);
        if (downstreamApiOptionsOverride is not null)
        {
            DownstreamApiOptionsReadOnlyHttpMethod options2 = new(
                options,
                httpMethod?.Method ?? HttpMethod.Get.Method
                );
            downstreamApiOptionsOverride(options2);
            options = options2;
        }
        (bool isSerializedContent, HttpContent? content) =
            input is null
            ? (false, null)
            : input is HttpContent inputContent
            ? (false, inputContent) 
            : options.Serializer is not null
            ? (true, options.Serializer(input))
            : inputTypeInfo is not null
#pragma warning disable CA2000 // Dispose objects before losing scope
            ? (true, JsonContent.Create(input, inputTypeInfo))
            : (true, JsonContent.Create(input));
#pragma warning restore CA2000 // Dispose objects before losing scope
        try
        {
            using HttpResponseMessage httpRespMsg = await CallApiAsync(
                httpMethod,
                options,
                serviceName,
                user,
                content,
                callMode,
                cancellationToken
                ).ConfigureAwait(continueOnCapturedContext: false);
            switch (responseHandling)
            {
                case ResponseHandlingMode.Ignore:
                    await httpRespMsg.Content.CopyToAsync(
                    Stream.Null
#if NET
                    , cancellationToken
#endif
                    ).ConfigureAwait(continueOnCapturedContext: false);
                    return null;
                case ResponseHandlingMode.Message:
                    return (TOutput)(object)httpRespMsg;
                default:
                    return options.Deserializer is not null
                    ? (TOutput?)options.Deserializer(httpRespMsg.Content)
                    : outputTypeInfo is not null
                    ? await httpRespMsg.Content.ReadFromJsonAsync(
                        outputTypeInfo,
                        cancellationToken
                        ).ConfigureAwait(continueOnCapturedContext: false)
                    : await httpRespMsg.Content.ReadFromJsonAsync<TOutput>(
                        cancellationToken
                        ).ConfigureAwait(continueOnCapturedContext: false);
            }
        }
        finally
        {
            if (isSerializedContent)
            {
                content?.Dispose();
            }
        }
    }

    private Task<HttpResponseMessage> CallApiAsync(
        HttpMethod? httpMethod,
        string? serviceName,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CallApiMode callMode = default,
        CancellationToken cancellationToken = default)
        => CallApiAsync<object?, HttpResponseMessage>(
            httpMethod,
            serviceName,
            input: null,
            inputTypeInfo: null,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            callMode,
            ResponseHandlingMode.Message,
            cancellationToken
            )!;

    private Task<HttpResponseMessage> CallApiAsync(
        HttpMethod? httpMethod,
        string? serviceName,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        HttpContent? content = null,
        CallApiMode callMode = default,
        CancellationToken cancellationToken = default)
        => CallApiAsync<HttpContent?, HttpResponseMessage>(
            httpMethod,
            serviceName,
            content,
            inputTypeInfo: null,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            callMode,
            ResponseHandlingMode.Message,
            cancellationToken
            )!;

    private Task<TOutput?> CallApiAsync<TInput, TOutput>(
        HttpMethod? httpMethod,
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CallApiMode callMode = default,
        ResponseHandlingMode responseHandling = default,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            httpMethod,
            serviceName,
            input,
            inputTypeInfo: null,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            callMode,
            responseHandling,
            cancellationToken
            );


    public Task<HttpResponseMessage> CallApiAsync(
        DownstreamApiOptions downstreamApiOptions,
        ClaimsPrincipal? user = null,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
        => CallApiAsync(
            options: downstreamApiOptions ?? throw new ArgumentNullException(
                paramName: nameof(downstreamApiOptions)
                ),
            user: user,
            content: content,
            cancellationToken: cancellationToken,
            httpMethod: null,
            serviceName: default,
            callMode: CallApiMode.Unspecified
            );

    public Task<HttpResponseMessage> CallApiAsync(
        string? serviceName,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
        => CallApiAsync(
            httpMethod: null,
            serviceName,
            downstreamApiOptionsOverride,
            user,
            content,
            callMode: CallApiMode.Unspecified,
            cancellationToken
            );

    public Task<HttpResponseMessage> CallApiForUserAsync(
        string? serviceName,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
        => CallApiAsync(
            httpMethod: null,
            serviceName,
            downstreamApiOptionsOverride,
            user,
            content,
            CallApiMode.ForUser,
            cancellationToken
            );

    public Task<HttpResponseMessage> CallApiForAppAsync(
        string? serviceName,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
        => CallApiAsync(
            httpMethod: null,
            serviceName,
            downstreamApiOptionsOverride,
            user: null,
            content,
            CallApiMode.ForApp,
            cancellationToken
            );

    public Task<TOutput?> CallApiForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            httpMethod: null,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForUserAsync<TOutput>(
        string serviceName,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            httpMethod: null,
            serviceName,
            input: null,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            httpMethod: null,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForAppAsync<TOutput>(
        string serviceName,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            httpMethod: null,
            serviceName,
            input: null,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            httpMethod: null,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForUserAsync<TOutput>(
        string serviceName,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            httpMethod: null,
            serviceName,
            input: null,
            inputTypeInfo: null,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            httpMethod: null,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> CallApiForAppAsync<TOutput>(
        string serviceName,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptions>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            httpMethod: null,
            serviceName,
            input: null,
            inputTypeInfo: null,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public Task<TOutput?> GetForUserAsync<TOutput>(
        string? serviceName,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            HttpMethod.Get,
            serviceName,
            input: null,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> GetForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Get,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> GetForAppAsync<TOutput>(
        string? serviceName,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            HttpMethod.Get,
            serviceName,
            input: null,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> GetForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Get,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task PostForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Post,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PostForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Post,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task PostForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Post,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PostForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Post,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task PutForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Put,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PutForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Put,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task PutForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Put,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PutForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Put,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task PatchForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PatchForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task PatchForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PatchForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task DeleteForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Delete,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> DeleteForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Delete,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task DeleteForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Delete,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> DeleteForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<TInput, TOutput>(
            HttpMethod.Delete,
            serviceName,
            input,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public Task<TOutput?> GetForUserAsync<TOutput>(
        string? serviceName,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            HttpMethod.Get,
            serviceName,
            null,
            inputTypeInfo: null,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> GetForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Get,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> GetForAppAsync<TOutput>(
        string? serviceName,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync<object?, TOutput>(
            HttpMethod.Get,
            serviceName,
            null,
            inputTypeInfo: null,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public Task<TOutput?> GetForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Get,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task PostForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Post,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PostForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Post,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task PostForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Post,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PostForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Post,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task PutForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Put,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PutForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Put,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task PutForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Put,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PutForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Put,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task PatchForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PatchForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task PatchForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> PatchForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
#if NET
            HttpMethod.Patch,
#else
            HttpPatch,
#endif
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );


    public async Task DeleteForUserAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Delete,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> DeleteForUserAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Delete,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user,
            CallApiMode.ForUser,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );

    public async Task DeleteForAppAsync<TInput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default)
        => _ = await CallApiAsync<TInput, object>(
            HttpMethod.Delete,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputTypeInfo: null,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Ignore,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

    public Task<TOutput?> DeleteForAppAsync<TInput, TOutput>(
        string? serviceName,
        TInput input,
        JsonTypeInfo<TInput> inputJsonTypeInfo,
        JsonTypeInfo<TOutput> outputJsonTypeInfo,
        Action<DownstreamApiOptionsReadOnlyHttpMethod>? downstreamApiOptionsOverride = null,
        CancellationToken cancellationToken = default) where TOutput : class
        => CallApiAsync(
            HttpMethod.Delete,
            serviceName,
            input,
            inputJsonTypeInfo,
            outputJsonTypeInfo,
            downstreamApiOptionsOverride,
            user: null,
            CallApiMode.ForApp,
            ResponseHandlingMode.Deserialize,
            cancellationToken
            );
}
