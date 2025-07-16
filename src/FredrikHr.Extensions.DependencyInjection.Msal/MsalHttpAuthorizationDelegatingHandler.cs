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
        request.TryGetOptionByType(out MsalHttpAuthorizationResourceRegistry? resourceRegistry);
        resourceRegistry ??= GetServiceProvider(request).GetService<
            MsalHttpAuthorizationResourceRegistry
            >();
        resource = resourceRegistry?.GetResource(request.RequestUri);
        return resource;
    }

    private ICollection<string>? GetMsalScopePermissions(HttpRequestMessage request)
    {
        return null;
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

    private static Func<DeviceCodeResult, Task> GetDeviceCodeResultCallback(
        IServiceProvider serviceProvider
        )
    {
        var callback = serviceProvider.GetService<Func<DeviceCodeResult, Task>>()
            ?? (deviceCodeResult =>
            {
                const Microsoft.Extensions.Logging.LogLevel critLevel =
                    Microsoft.Extensions.Logging.LogLevel.Critical;
                var logger = serviceProvider.GetRequiredService<ILogger<IPublicClientApplication>>();
                if (!logger.IsEnabled(critLevel))
                {
                    return Task.CompletedTask;
                }

                logger.Log(
                    critLevel,
                    new EventId(1, nameof(DeviceCodeResult)),
                    exception: null,
                    state: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { nameof(DeviceCodeResult), deviceCodeResult },
                        { nameof(DeviceCodeResult.ClientId), deviceCodeResult.ClientId },
                        { nameof(DeviceCodeResult.ExpiresOn), deviceCodeResult.ExpiresOn },
                        { nameof(DeviceCodeResult.Scopes), deviceCodeResult.Scopes },
                        { nameof(DeviceCodeResult.Message), deviceCodeResult.Message },
                        { nameof(DeviceCodeResult.UserCode), deviceCodeResult.UserCode },
                        { nameof(DeviceCodeResult.VerificationUrl), deviceCodeResult.VerificationUrl },
                    },
                    formatter: static (state, _) => state[nameof(DeviceCodeResult.Message)]?.ToString() ?? ""
                    );
                return Task.CompletedTask;
            });
        return callback;
    }

    private TClient GetMsalClient<TClient, TAlternate>(
        HttpRequestMessage request,
        IServiceProvider serviceProvider
        )
        where TClient : class, IApplicationBase
        where TAlternate : TClient
    {
        request.TryGetOptionByType(out TClient? msalClient);
        msalClient ??= request.TryGetOptionByType(out TAlternate? msalClientAlt)
            ? msalClientAlt
            : null;
        msalClient ??= serviceProvider.GetRequiredService<
                IOptionsMonitor<TClient>
                >().Get(name);
        return msalClient;
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
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenByAuthorizationCode(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IConfidentialClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);
        string? authorizationCode = request.GetMsalAuthorizationCode();

        var acquireTokenBuilder = msalClient.AcquireTokenByAuthorizationCode(scopes, authorizationCode);
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
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
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenOnBehalfOf(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IConfidentialClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);
        UserAssertion? assertion = request.GetMsalUserAssertion();

        var acquireTokenBuilder = msalClient.AcquireTokenOnBehalfOf(scopes, assertion);
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
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
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenWithDeviceCode(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        PublicClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);
        var deviceCodeResultCallback = GetDeviceCodeResultCallback(serviceProvider);

        var acquireTokenBuilder = msalClient.AcquireTokenWithDeviceCode(scopes, deviceCodeResultCallback);
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenByIntegratedWindowsAuth(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        PublicClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = GetMsalScopes(request);

        var acquireTokenBuilder = msalClient.AcquireTokenByIntegratedWindowsAuth(scopes);
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private Task<AuthenticationResult> AcquireTokenByUsernamePassword(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        IPublicClientApplication msalClient,
        CancellationToken cancelToken
        )
    {
        IEnumerable<string>? scopes = request.GetMsalScopes();
        NetworkCredential? credential = request.GetMsalUsernamePasswordCredential();
        (string? username, string? password) = (credential?.UserName, credential?.Password);

        var acquireTokenBuilder = msalClient.AcquireTokenByUsernamePassword(scopes, username, password);
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
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
        ConfigureAcquireTokenBuilder(request, serviceProvider, acquireTokenBuilder);
        return acquireTokenBuilder.ExecuteAsync(cancelToken);
    }

    private void ConfigureAcquireTokenBuilder<TBuilder>(
        HttpRequestMessage request,
        IServiceProvider serviceProvider,
        TBuilder acquireTokenBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        IEnumerable<IConfigureOptions<TBuilder>> acquireTokenConfigureOptions =
            request.GetOptionEnumerableByType<
            IConfigureOptions<TBuilder>
            >().Concat(serviceProvider.GetServices<
                IConfigureOptions<TBuilder>
            >());
        IEnumerable<IPostConfigureOptions<TBuilder>> acquireTokenPostConfigureOptions =
            request.GetOptionEnumerableByType<
            IPostConfigureOptions<TBuilder>
            >().Concat(serviceProvider.GetServices<
                IPostConfigureOptions<TBuilder>
            >());
        IValidateOptions<TBuilder>[] acquireTokenValidations =
            [
                .. request.GetOptionEnumerableByType<
                    IValidateOptions<TBuilder>
                    >(),
                .. serviceProvider.GetServices<
                    IValidateOptions<TBuilder>
                    >(),
            ];
        foreach (IConfigureOptions<TBuilder> configureAcquireTokenBuilder in acquireTokenConfigureOptions)
        {
            switch (configureAcquireTokenBuilder)
            {
                case IConfigureNamedOptions<TBuilder> namedConfigureAcquireTokenBuilder:
                    namedConfigureAcquireTokenBuilder.Configure(name, acquireTokenBuilder);
                    break;
                default:
                    configureAcquireTokenBuilder.Configure(acquireTokenBuilder);
                    break;
            }
        }
        foreach (IPostConfigureOptions<TBuilder> configureAcquireTokenBuilder in acquireTokenPostConfigureOptions)
        {
            configureAcquireTokenBuilder.PostConfigure(name, acquireTokenBuilder);
        }
        if (acquireTokenValidations.Length > 0)
        {
            List<string> failures = [];
            foreach (IValidateOptions<TBuilder> validateAcquireTokenBuilder in acquireTokenValidations)
            {
                ValidateOptionsResult result = validateAcquireTokenBuilder.Validate(name, acquireTokenBuilder);
                if (result is not null && result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }
            if (failures.Count > 0)
            {
                throw new OptionsValidationException(
                    name ?? Options.DefaultName,
                    typeof(TBuilder),
                    failures
                    );
            }
        }
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
