using System.Net;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Identity.Client;

public static class MsalHttpAuthorizationExtensions
{
    internal static IDictionary<string, object?> GetOptions(
        this HttpRequestMessage request
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
        return options;
    }

    internal static bool TryGetOptionByType<T>(
        this HttpRequestMessage request,
        out T? optionValue
        )
    {
        var options = request.GetOptions();
        optionValue = default;
        if (options.TryGetValue(typeof(T).FullName!, out var optionObjectValue))
        {
            if (optionObjectValue is null)
            {
                return true;
            }
            else if (optionObjectValue is T optionTValue)
            {
                optionValue = optionTValue;
                return true;
            }
        }
        return false;
    }

    internal static IEnumerable<T> GetOptionEnumerableByType<T>(
        this HttpRequestMessage request
        )
    {
        var options = request.GetOptions();
        var optionKey = typeof(T).FullName!;
        options.TryGetValue(optionKey, out var optionObjectValue);
        return optionObjectValue switch
        {
            IEnumerable<T> optionEnumerable => optionEnumerable,
            T optionSingleValue => [optionSingleValue],
            _ => [],
        };
    }

    internal static void SetOptionByType<T>(
        this HttpRequestMessage request,
        T? optionValue
        )
    {
        var options = request.GetOptions();
        string? optionKey = typeof(T).FullName ?? optionValue?.GetType().FullName;
        if (optionKey is null) return;
        options[optionKey] = optionValue;
    }

    internal static void AddOptionByType<T>(
        this HttpRequestMessage request,
        T? optionValue
        )
    {
        var options = request.GetOptions();
        string? optionKey = typeof(T).FullName ?? optionValue?.GetType().FullName;
        if (optionKey is null) return;
        options.Add(optionKey, optionValue);
    }

    public static void AddServiceScope(
        this HttpRequestMessage request,
        IServiceScope serviceScope
        ) => request.SetOptionByType(serviceScope);

    public static void AddServiceProvider(
        this HttpRequestMessage request,
        IServiceProvider serviceProvider
        ) => request.SetOptionByType(serviceProvider);

    public static IServiceScope? GetServiceScope(
        this HttpRequestMessage request
        ) => request.TryGetOptionByType(out IServiceScope? serviceScope)
            ? serviceScope : null;

    public static IServiceProvider? GetServiceProvider(
        this HttpRequestMessage request
        ) => request.GetServiceScope() is { ServiceProvider: IServiceProvider scopeServiceProvider }
            ? scopeServiceProvider
            : request.TryGetOptionByType(out IServiceProvider? serviceProvider)
            ? serviceProvider
            : null;

    public static void SetMsalAccount(
        this HttpRequestMessage request,
        IAccount account
        ) => request.SetOptionByType(account);

    public static IAccount? GetMsalAccount(
        this HttpRequestMessage request
        ) => request.TryGetOptionByType(out IAccount? account)
            ? account : null;

    public static IAccount? GetMsalAccount(
        this HttpResponseMessage response
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        _ = response ?? throw new ArgumentNullException(nameof(response));
#endif
        return GetMsalAccount(response.RequestMessage!);
    }

    public const string ResourceOptionsKey = "Microsoft.Identity.Client.Resource";

    public static void SetMsalResource(
        this HttpRequestMessage request,
        string resource
        ) => request.GetOptions()[ResourceOptionsKey] = resource;

    public static string? GetMsalResource(
        this HttpRequestMessage request
        ) => request.GetOptions().TryGetValue(ResourceOptionsKey, out var optionObjectValue)
            ? optionObjectValue as string : null;

    public const string ScopesOptionsKey = "Microsoft.Identity.Client.Scopes";

    public static void SetMsalScopes(
        this HttpRequestMessage request,
        IEnumerable<string> scopes
        ) => request.GetOptions()[ScopesOptionsKey] = scopes;

    public static void AddMsalScopes(
        this HttpRequestMessage request,
        params IEnumerable<string> scopes
        )
    {
        var existingScopes = request.GetMsalScopes();
        var combinedScopes = existingScopes switch
        {
            ICollection<string> existingScopeCollection => existingScopeCollection,
            _ => [.. existingScopes],
        };
        if (combinedScopes is List<string> combinedScopeList)
        {
            combinedScopeList.AddRange(scopes ?? []);
        }
        else
        {
            foreach (var scope in scopes ?? [])
            {
                combinedScopes.Add(scope);
            }
        }
        request.SetMsalScopes(combinedScopes);
    }

    public static void AddMsalScopes(
        this HttpRequestMessage request,
        params ReadOnlySpan<string> scopes
        )
    {
        var existingScopes = request.GetMsalScopes();
        var combinedScopes = existingScopes switch
        {
            ICollection<string> existingScopeCollection => existingScopeCollection,
            _ => [.. existingScopes],
        };
        if (combinedScopes is List<string> combinedScopeList)
        {
            int combinedLength = combinedScopeList.Count + scopes.Length;
            if (combinedScopeList.Capacity < combinedLength)
                combinedScopeList.Capacity = combinedLength;
        }
        foreach (ref readonly string scope in scopes)
        {
            combinedScopes.Add(scope);
        }
        request.SetMsalScopes(combinedScopes);
    }

    public static IEnumerable<string> GetMsalScopes(
        this HttpRequestMessage request
        )
    {
        return request.GetOptions().TryGetValue(ScopesOptionsKey, out var optionsObjectValue)
            ? optionsObjectValue switch
            {
                IEnumerable<string> optionsEnumerable => optionsEnumerable,
                string optionsSingleValue => [optionsSingleValue],
                _ => [],
            } : [];
    }

    public const string LoginHintOptionsKey = "Microsoft.Identity.Client.LoginHint";

    public static void SetMsalLoginHint(
        this HttpRequestMessage request,
        string loginHint
        )
    {
        request.GetOptions()[LoginHintOptionsKey] = loginHint;
    }

    public static string? GetMsalLoginHint(
        this HttpRequestMessage request
        )
    {
        string? optionsValue = request.GetOptions()
            .TryGetValue(LoginHintOptionsKey, out var optionObjectValue)
            ? optionObjectValue as string
            : null;
        return optionsValue;
    }

    public const string AuthorizationCodeOptionsKey = "Microsoft.Identity.Client.AuthorizationCode";

    public static void SetMsalAuthorizationCode(
        this HttpRequestMessage request,
        string authorizationCode
        )
    {
        request.GetOptions()[AuthorizationCodeOptionsKey] = authorizationCode;
    }

    public static string? GetMsalAuthorizationCode(
        this HttpRequestMessage request
        )
    {
        string? optionsValue = request.GetOptions()
            .TryGetValue(AuthorizationCodeOptionsKey, out var optionObjectValue)
            ? optionObjectValue as string
            : null;
        return optionsValue;
    }

    public static void SetMsalUserAssertion(
        this HttpRequestMessage request,
        UserAssertion userAssertion
        ) => request.SetOptionByType(userAssertion);

    public static UserAssertion? GetMsalUserAssertion(
        this HttpRequestMessage request
        ) => request.TryGetOptionByType(out UserAssertion? userAssertion)
            ? userAssertion : null;

    public const string UsernamePasswordCredentialOptionsKey = "Microsoft.Identity.Client.UsernamePasswordCredential";

    public static void SetMsalUsernamePasswordCredential(
        this HttpRequestMessage request,
        NetworkCredential credential
        )
    {
        request.GetOptions()[UsernamePasswordCredentialOptionsKey] = credential;
    }

    public static void SetMsalUsernamePasswordCredential(
        this HttpRequestMessage request,
        string username,
        string password
        )
    {
        request.GetOptions()[UsernamePasswordCredentialOptionsKey] =
            new NetworkCredential(username, password);
    }

    public static NetworkCredential? GetMsalUsernamePasswordCredential(
        this HttpRequestMessage request
        )
    {
        return request.GetOptions().TryGetValue(
            UsernamePasswordCredentialOptionsKey,
            out var optionObjectValue
            ) ? optionObjectValue as NetworkCredential : null;
    }
}