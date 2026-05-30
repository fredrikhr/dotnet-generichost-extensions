using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public class MsalHttpAuthorizationDelegatingHandler(
    string? name,
    IServiceProvider serviceProvider
    ) : DelegatingHandler()
{
    private (string? resource, string[]? scopes) _defaultResourceScopes;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private bool ShouldHandleRequest(HttpRequestMessage request)
    {
        if (request.TryGetOptionByType<MsalHttpClientFactoryHttpHandler>(out _))
            return false;

        if (GetMsalScopes(request) is not IEnumerable<string> msalScopes)
        {
            return false;
        }
        request.SetMsalScopes(msalScopes);

        return true;
    }

    private IServiceProvider GetServiceProvider(HttpRequestMessage request)
    {
        return request.GetMsalServiceProvider() ?? _serviceProvider;
    }

    private string? GetMsalResource(HttpRequestMessage request)
    {
        string? resource = request.GetMsalResource();
        if (resource is not null) return resource;
        request.TryGetOptionByType(out MsalResourceScopeRegistry? resourceRegistry);
        resourceRegistry ??= GetServiceProvider(request).GetService<
            MsalResourceScopeRegistry
            >();
        resource = resourceRegistry?.GetResource(request.RequestUri);
        return resource;
    }

    private static ICollection<string>? GetMsalScopePermissions(HttpRequestMessage request)
    {
        ICollection<string>? scopes = request.GetMsalPermissionScopes() switch
        {
            string[] requestScopesArray => requestScopesArray,
            ICollection<string> requestScopesCollection => requestScopesCollection,
            null => null,
            var requestScopesEnumerable => [.. requestScopesEnumerable],
        };
        return scopes is { Count: > 0 } ? scopes : null;
    }

    private static bool IsOpenIdSpecialScope(string scope)
    {
        const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
        return "offline_access".Equals(scope, cmp);
    }

    private IEnumerable<string>? GetMsalScopes(HttpRequestMessage request)
    {
        ICollection<string>? scopes = request.GetMsalScopes() switch
        {
            string[] requestScopesArray => requestScopesArray,
            ICollection<string> requestScopesCollection => requestScopesCollection,
            null => null,
            var requestScopesEnumerable => [.. requestScopesEnumerable],
        };
        if (scopes is { Count: > 0 }) return scopes;
        string? resource = GetMsalResource(request);
        ICollection<string>? permissions = GetMsalScopePermissions(request);
        if (resource is null) return permissions;
        if (permissions is not { Count: > 0 })
        {
            const StringComparison cmp = StringComparison.Ordinal;
            (string? defaultResource, string[]? defaultScopes) = _defaultResourceScopes;
            if (resource.Equals(defaultResource, cmp) && defaultScopes is not null)
                return defaultScopes;
            defaultScopes = [$"{resource}/.default"];
            _defaultResourceScopes = (resource, defaultScopes);
            return defaultScopes;
        }
        var scopesArray = new string[permissions.Count];
        int scopeIdx;
        if (permissions is string[] permissionsArray)
        {
            scopeIdx = 0;
            foreach (string permission in permissionsArray)
            {
                scopesArray[scopeIdx] = GetResourceScopeFromPermissionScope(
                    resource,
                    permission
                    );
                scopeIdx++;
            }
        }
        else
        {
            scopeIdx = 0;
            foreach (string permission in permissions)
            {
                scopesArray[scopeIdx] = GetResourceScopeFromPermissionScope(
                    resource,
                    permission
                    );
                scopeIdx++;
            }
        }
        return scopesArray;

        static string GetResourceScopeFromPermissionScope(string resource, string scope)
        {
            return IsOpenIdSpecialScope(scope) ? scope : $"{resource}/{scope}";
        }
    }

    private Task<AuthenticationResult> AcquireTokenSilent(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IClientApplicationBase msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);
        IAccount? account = request.GetMsalAccount();
        AcquireTokenSilentParameterBuilder acquireTokenBuilder;
        if (account is null)
        {
            string? loginHint = request.GetMsalLoginHint();
            acquireTokenBuilder = msalClient.AcquireTokenSilent(scopes, loginHint);
        }
        else
            acquireTokenBuilder = msalClient.AcquireTokenSilent(scopes, account);
        acquireTokenBuilder.ApplyOptions(request, serviceProvider, name);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenForClient(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IConfidentialClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);

        var acquireTokenBuilder = msalClient.AcquireTokenForClient(scopes);
        acquireTokenBuilder.ApplyOptions(request, serviceProvider, name);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenInteractive(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IPublicClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);

        var acquireTokenBuilder = msalClient.AcquireTokenInteractive(scopes);
        acquireTokenBuilder.ApplyOptions(request, serviceProvider, name);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenForManagedIdentity(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IManagedIdentityApplication msalClient,
        CancellationToken cancelToken
        )
    {
        string? resource = GetMsalResource(request)
            ?? (GetMsalScopes(request) ?? []).FirstOrDefault();

        var acquireTokenBuilder = msalClient.AcquireTokenForManagedIdentity(resource);
        acquireTokenBuilder.ApplyOptions(request, serviceProvider, name);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private static void ApplyAuthenticationResult(
        HttpRequestMessage request,
        AuthenticationResult? authResult
        )
    {
        if (authResult is null) return;

        request.Headers.Add(
            nameof(request.Headers.Authorization),
            authResult.CreateAuthorizationHeader()
            );
        request.SetOptionByType(authResult.Account);
        request.SetOptionByType(authResult);
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

        bool shouldHandle = ShouldHandleRequest(request);
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

        bool shouldHandle = ShouldHandleRequest(request);
        if (shouldHandle)
        {

        }

        return base.Send(request, cancellationToken);
    }
#endif
}
