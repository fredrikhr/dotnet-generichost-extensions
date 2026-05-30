using System.Net;

using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;

namespace FredrikHr.Extensions.DependencyInjection.Msal;

public class MsalHttpMessageHandler : DelegatingHandler
{
    private readonly IAuthorizationHeaderProvider _headerProvider;
    private readonly MsalHttpMessageHandlerOptions? _defaultOptions;

    public MsalHttpMessageHandler(
        IAuthorizationHeaderProvider headerProvider,
        MsalHttpMessageHandlerOptions? defaultOptions = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(headerProvider);
#else
        _ = headerProvider ?? throw new ArgumentNullException(nameof(headerProvider));
#endif

        _headerProvider = headerProvider;
        _defaultOptions = defaultOptions;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        _ = request ?? throw new ArgumentNullException(nameof(request));
#endif

        // Get per-request options or use default
        MsalHttpMessageHandlerOptions? options = request.GetAuthenticationOptions()
            ?? _defaultOptions
            ?? throw new InvalidOperationException(
                "Authentication options must be configured either in default options or per-request."
                );

        // Get scopes from options
        if (options.Scopes is not { Count: >0 } scopes)
        {
            throw new InvalidOperationException(
                "Authentication scopes must be configured in the provided authentication options."
                );
        }

        // Send the request with authentication
        HttpResponseMessage response = await SendWithAuthenticationAsync(
            request,
            options,
            scopes,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

        // Handle WWW-Authenticate challenge if present
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Use MSAL's WWW-Authenticate parser to extract claims from challenge headers
            string? challengeClaims = WwwAuthenticateParameters
                .GetClaimChallengeFromResponseHeaders(response.Headers);
            if (string.IsNullOrEmpty(challengeClaims))
            {
                // Create a new options instance with the challenge claims
                MsalHttpMessageHandlerOptions challengeOptions =
                    CreateOptionsWithChallengeClaims(options, challengeClaims);

                // Clone the original request for retry
                using var retryRequest = await CloneHttpRequestMessageAsync(request)
                    .ConfigureAwait(continueOnCapturedContext: false);

                // Attempt to get a new token with the challenge claims
                HttpResponseMessage retryResponse = await SendWithAuthenticationAsync(
                    retryRequest,
                    challengeOptions,
                    scopes,
                    cancellationToken
                    ).ConfigureAwait(continueOnCapturedContext: false);

                response.Dispose();
                return retryResponse;
            }
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendWithAuthenticationAsync(
        HttpRequestMessage request,
        MsalHttpMessageHandlerOptions options,
        IList<string> scopes,
        CancellationToken cancellationToken
        )
    {
        // Acquire authorization header
        string authHeader = await _headerProvider.CreateAuthorizationHeaderAsync(
            scopes,
            options,
            cancellationToken: cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);

        // Remove existing authorization header if present
        if (request.Headers.Contains("Authorization"))
        {
            request.Headers.Remove("Authorization");
        }

        // Add the authorization header
        request.Headers.Add("Authorization", authHeader);

        // Send the request through the normal handler pipeline
        return await base.SendAsync(request, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// Creates a new options instance with challenge claims added.
    /// </summary>
    /// <param name="originalOptions">The original authentication options.</param>
    /// <param name="challengeClaims">The claims from the WWW-Authenticate challenge.</param>
    /// <returns>A new options instance with challenge claims configured.</returns>
    private static MsalHttpMessageHandlerOptions CreateOptionsWithChallengeClaims(
        MsalHttpMessageHandlerOptions originalOptions,
        string challengeClaims)
    {
        var challengeOptions = new MsalHttpMessageHandlerOptions(originalOptions);

        // Set challenge claims and force refresh
        if (challengeOptions.AcquireTokenOptions != null)
        {
            challengeOptions.AcquireTokenOptions.Claims = challengeClaims;
            challengeOptions.AcquireTokenOptions.ForceRefresh = true;
        }
        else
        {
            challengeOptions.AcquireTokenOptions = new AcquireTokenOptions
            {
                Claims = challengeClaims,
                ForceRefresh = true
            };
        }

        return challengeOptions;
    }

    /// <summary>
    /// Clones an HttpRequestMessage for retry scenarios.
    /// </summary>
    /// <param name="originalRequest">The original request to clone.</param>
    /// <returns>A cloned HttpRequestMessage.</returns>
    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(
        HttpRequestMessage originalRequest
        )
    {
        HttpRequestMessage clonedRequest =
            new(originalRequest.Method, originalRequest.RequestUri);

        // Copy headers
        foreach (var header in originalRequest.Headers)
        {
            // Skip Authorization header as it will be set by the handler
            if (header.Key != "Authorization")
            {
                clonedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy content if present
        if (originalRequest.Content != null)
        {
            var contentBytes = await originalRequest.Content
                .ReadAsByteArrayAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            clonedRequest.Content = new ByteArrayContent(contentBytes);

            // Copy content headers
            foreach (var header in originalRequest.Content.Headers)
            {
                clonedRequest.Content.Headers
                    .TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy properties/options (excluding authentication options which will be set separately)
        // Note: We don't copy options to avoid complications with typed keys.
        // Most HttpClient scenarios don't rely on copying all options to retry requests.
#if !NET5_0_OR_GREATER
        foreach (var property in originalRequest.Properties)
        {
            // Skip our authentication options as they will be set separately
            if (!property.Key.Equals("Microsoft.Identity.AuthenticationOptions", StringComparison.Ordinal))
            {
                clonedRequest.Properties[property.Key] = property.Value;
            }
        }
#endif

        return clonedRequest;
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA2227: Collection properties should be read only",
    Justification = nameof(Microsoft.Extensions.Options)
    )]
public class MsalHttpMessageHandlerOptions : AuthorizationHeaderProviderOptions
{
    public MsalHttpMessageHandlerOptions() : base() { }

    public MsalHttpMessageHandlerOptions(MsalHttpMessageHandlerOptions other)
        : base(other)
    {
        Scopes = other?.Scopes ?? [];
    }

    /// <summary>
    /// Gets or sets the scopes to request for the token.
    /// </summary>
    /// <value>
    /// A list of scopes required to access the target API.
    /// For instance, "user.read mail.read" for Microsoft Graph user permissions.
    /// For Microsoft Identity, in the case of application tokens (requested by the app on behalf of itself),
    /// there should be only one scope, and it should end with ".default" (e.g., "https://graph.microsoft.com/.default").
    /// </value>
    /// <example>
    /// <code>
    /// MsalHttpMessageHandlerOptions options = new()
    /// {
    ///     Scopes = {
    ///         "https://graph.microsoft.com/.default",
    ///         "https://myapi.domain.com/access",
    ///     },
    /// };
    /// </code>
    /// </example>
    /// <remarks>
    /// This property must contain at least one scope, or the <see cref="MsalHttpMessageHandler"/>
    /// will throw a <see cref="InvalidOperationException"/> when processing requests.
    /// </remarks>
    public IList<string> Scopes { get; set; } = [];
}

public static class MsalHttpRequestMessage
{
    public const string AuthenticationOptionsKey = "Microsoft.Identity.AuthenticationOptions";

    extension(HttpRequestMessage requestMessage)
    {
        public MsalHttpMessageHandlerOptions? GetAuthenticationOptions()
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(requestMessage);
#else
            _ = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
#endif

            _ = requestMessage.GetOptions().TryGetValue(
                AuthenticationOptionsKey,
                out object? options
                );
            return options as MsalHttpMessageHandlerOptions;
        }
    }
}