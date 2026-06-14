using System.Security.Claims;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity;

using BoundAuthorizationResult = OperationResult<AuthorizationHeaderInformation, AuthorizationHeaderError>;

public class TokenAcquirerAuthorizationHeaderProvider(
    ITokenAcquirer tokenAcquirer
    ) : IAuthorizationHeaderProvider, IBoundAuthorizationHeaderProvider
{
    internal const string TokenBindingProtocolScheme = "MTLS_POP";
    private readonly ITokenAcquirer _tokenAcquirer = tokenAcquirer
        ?? throw new ArgumentNullException(nameof(tokenAcquirer));

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0046: Convert to conditional expression",
        Justification = "improved readability"
        )]
    public Task<string> CreateAuthorizationHeaderAsync(
        IEnumerable<string> scopes,
        AuthorizationHeaderProviderOptions? options = null,
        ClaimsPrincipal? claimsPrincipal = null,
        CancellationToken cancellationToken = default
        )
    {
        if (options is { RequestAppToken: true } ||
            options is { AcquireTokenOptions.ManagedIdentity: not null }
            )
        {
            return scopes.SingleOrDefault() is { Length: > 0 } singlescope
                ? CreateAuthorizationHeaderForAppAsync(
                    singlescope,
                    options,
                    cancellationToken
                    )
                : throw new ArgumentException(
                    message: "Exactly one scope needs to be specifies when creating an Authorization header for an application",
                    paramName: nameof(scopes)
                    );
        }

        return CreateAuthorizationHeaderForUserAsync(
            scopes,
            options,
            claimsPrincipal,
            cancellationToken
            );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1725: Parameter names should match base declaration",
        Justification = $"{nameof(scope)}, {nameof(options)}"
        )]
    public async Task<string> CreateAuthorizationHeaderForAppAsync(
        string scope,
        AuthorizationHeaderProviderOptions? options = null,
        CancellationToken cancellationToken = default
        )
    {
        AcquireTokenResult result = await _tokenAcquirer.GetTokenForAppAsync(
            scope,
            options?.AcquireTokenOptions,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);
        return result.GetAuthorizationHeaderValue();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1725: Parameter names should match base declaration",
        Justification = nameof(options)
        )]
    public async Task<string> CreateAuthorizationHeaderForUserAsync(
        IEnumerable<string> scopes,
        AuthorizationHeaderProviderOptions? options = null,
        ClaimsPrincipal? claimsPrincipal = null,
        CancellationToken cancellationToken = default
        )
    {
        AcquireTokenResult result = await _tokenAcquirer.GetTokenForUserAsync(
            scopes,
            options?.AcquireTokenOptions,
            claimsPrincipal,
            cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);
        return result.GetAuthorizationHeaderValue();
    }

    public async Task<BoundAuthorizationResult> CreateBoundAuthorizationHeaderAsync(
            DownstreamApiOptions downstreamApiOptions,
            ClaimsPrincipal? claimsPrincipal = null,
            CancellationToken cancellationToken = default)
    {
        const StringComparison ordIgn = StringComparison.OrdinalIgnoreCase;
        IEnumerable<string> scopes = downstreamApiOptions?.Scopes ?? [];
        if (!TokenBindingProtocolScheme.Equals(downstreamApiOptions?.ProtocolScheme, ordIgn))
        {
            string headerValue = await CreateAuthorizationHeaderAsync(
                scopes,
                downstreamApiOptions,
                claimsPrincipal,
                cancellationToken
                ).ConfigureAwait(continueOnCapturedContext: false);
            return new(new AuthorizationHeaderInformation()
            {
                AuthorizationHeaderValue = headerValue,
                BindingCertificate = null,
            });
        }

        AcquireTokenResult tokenResult;
        if (downstreamApiOptions is { RequestAppToken: true } ||
            downstreamApiOptions is { AcquireTokenOptions.ManagedIdentity: not null }
            )
        {
            if (scopes.SingleOrDefault() is not { Length: > 0 } singlescope)
            {
                throw new ArgumentException(
                    message: "Exactly one scope needs to be specifies when creating an Authorization header for an application",
                    paramName: $"{nameof(downstreamApiOptions)}.{nameof(downstreamApiOptions.Scopes)}"
                    );
            }
            tokenResult = await _tokenAcquirer.GetTokenForAppAsync(
                singlescope,
                downstreamApiOptions.AcquireTokenOptions,
                cancellationToken
                ).ConfigureAwait(continueOnCapturedContext: false);
            return new(new AuthorizationHeaderInformation()
            {
                AuthorizationHeaderValue = tokenResult.GetAuthorizationHeaderValue(),
                BindingCertificate = tokenResult.BindingCertificate,
            });
        }

        tokenResult = await _tokenAcquirer.GetTokenForUserAsync(
                scopes,
                downstreamApiOptions?.AcquireTokenOptions,
                claimsPrincipal,
                cancellationToken
                ).ConfigureAwait(continueOnCapturedContext: false);
        return new(new AuthorizationHeaderInformation()
        {
            AuthorizationHeaderValue = tokenResult.GetAuthorizationHeaderValue(),
            BindingCertificate = tokenResult.BindingCertificate,
        });
    }
}
