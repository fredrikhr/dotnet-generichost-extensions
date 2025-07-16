using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

#if NET5_0_OR_GREATER
        return request.Options;
#else
        return request.Properties;
#endif
    }

    internal static bool TryGetOptionByType<T>(
        this HttpRequestMessage request,
        out T? optionValue
        )
    {
        IDictionary<string, object?> options = request.GetOptions();
        optionValue = default;
        if (options.TryGetValue(typeof(T).FullName!, out object? optionObjectValue))
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
        IDictionary<string, object?> options = request.GetOptions();
        string optionKey = typeof(T).FullName!;
        options.TryGetValue(optionKey, out object? optionObjectValue);
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
        IDictionary<string, object?> options = request.GetOptions();
        string optionKey = typeof(T).FullName!;
        options[optionKey] = optionValue;
    }

    internal static void AddOptionByType<T>(
        this HttpRequestMessage request,
        T? optionValue
        )
    {
        IDictionary<string, object?> options = request.GetOptions();
        string optionKey = typeof(T).FullName!;
        options.Add(optionKey, optionValue);
    }

    public static void SetMsalPublicClient(
        this HttpRequestMessage request,
        IPublicClientApplication client
        )
    {
        if (client is PublicClientApplication clientClass)
            request.AddOptionByType(clientClass);
        else
            request.AddOptionByType(client);
    }

    public static void SetMsalConfidentialClient(
        this HttpRequestMessage request,
        IConfidentialClientApplication client
        )
    {
        if (client is ConfidentialClientApplication clientClass)
            request.AddOptionByType(clientClass);
        else
            request.AddOptionByType(client);
    }

    public static void SetMsalManagedIdentityClient(
        this HttpRequestMessage request,
        IManagedIdentityApplication client
        )
    {
        if (client is ManagedIdentityApplication clientClass)
            request.AddOptionByType(clientClass);
        else
            request.AddOptionByType(client);
    }

    public const string ServiceScopeOptionsKey = "Microsoft.Identity.Client.ServiceScope";

    public static void SetMsalServiceScope(
        this HttpRequestMessage request,
        IServiceScope serviceScope
        ) => request.GetOptions()[ServiceScopeOptionsKey] = serviceScope;

    public static IServiceScope? GetMsalServiceScope(
        this HttpRequestMessage request
        ) => request.GetOptions().TryGetValue(ServiceScopeOptionsKey, out object? optionObjectValue)
            ? optionObjectValue as IServiceScope : null;

    public const string ServiceProviderOptionsKey = "Microsoft.Identity.Client.ServiceProvider";

    public static void SetMsalServiceProvider(
        this HttpRequestMessage request,
        IServiceProvider serviceProvider
        ) => request.GetOptions()[ServiceProviderOptionsKey] = serviceProvider;

    public static IServiceProvider? GetMsalServiceProvider(
        this HttpRequestMessage request
        ) => request.GetMsalServiceScope() is { ServiceProvider: IServiceProvider scopeServiceProvider }
            ? scopeServiceProvider
            : request.GetOptions().TryGetValue(ServiceProviderOptionsKey, out object? optionObjectValue)
            ? optionObjectValue as IServiceProvider
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
        ) => request.GetOptions().TryGetValue(ResourceOptionsKey, out object? optionObjectValue)
            ? optionObjectValue as string : null;

    public const string PermissionScopesOptionsKey = "Microsoft.Identity.Client.Permissions";

    public static void SetMsalPermissionScopes(
        this HttpRequestMessage request,
        IEnumerable<string> scopes
        ) => request.GetOptions()[PermissionScopesOptionsKey] = scopes;

    public static void AddMsalPermissionScopes(
        this HttpRequestMessage request,
        params IEnumerable<string> scopes
        )
    {
        IEnumerable<string> existingScopes = request.GetMsalScopes();
        ICollection<string> combinedScopes = existingScopes switch
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
            foreach (string scope in scopes ?? [])
            {
                combinedScopes.Add(scope);
            }
        }
        request.SetMsalPermissionScopes(combinedScopes);
    }

    public static void AddMsalPermissionScopes(
        this HttpRequestMessage request,
        params ReadOnlySpan<string> scopes
        )
    {
        IEnumerable<string> existingScopes = request.GetMsalPermissionScopes();
        ICollection<string> combinedScopes = existingScopes switch
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
        request.SetMsalPermissionScopes(combinedScopes);
    }

    public static IEnumerable<string> GetMsalPermissionScopes(
        this HttpRequestMessage request
        )
    {
        return request.GetOptions().TryGetValue(PermissionScopesOptionsKey, out object? optionsObjectValue)
            ? optionsObjectValue switch
            {
                IEnumerable<string> optionsEnumerable => optionsEnumerable,
                string optionsSingleValue => [optionsSingleValue],
                _ => [],
            } : [];
    }

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
        IEnumerable<string> existingScopes = request.GetMsalScopes();
        ICollection<string> combinedScopes = existingScopes switch
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
            foreach (string scope in scopes ?? [])
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
        IEnumerable<string> existingScopes = request.GetMsalScopes();
        ICollection<string> combinedScopes = existingScopes switch
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
        return request.GetOptions().TryGetValue(ScopesOptionsKey, out object? optionsObjectValue)
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
            .TryGetValue(LoginHintOptionsKey, out object? optionObjectValue)
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
            .TryGetValue(AuthorizationCodeOptionsKey, out object? optionObjectValue)
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
            out object? optionObjectValue
            ) ? optionObjectValue as NetworkCredential : null;
    }

    public static void AddMsalAcquireTokenConfiguration<TBuilder>(
        this HttpRequestMessage request,
        IConfigureOptions<TBuilder> configureBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        IDictionary<string, object?> options = request.GetOptions();
        var optionsKey = typeof(IConfigureOptions<TBuilder>).FullName!;
        options.TryGetValue(optionsKey, out object? optionsExisting);
        ICollection<IConfigureOptions<TBuilder>> optionsCollection = optionsExisting switch
        {
            IConfigureOptions<TBuilder> optionsExistingSingle => [optionsExistingSingle],
            ICollection<IConfigureOptions<TBuilder>> optionsExistingCollection => optionsExistingCollection,
            IEnumerable<IConfigureOptions<TBuilder>> optionsExistingEnumerable => [.. optionsExistingEnumerable],
            _ => []
        };
        optionsCollection.Add(configureBuilder);
        options[optionsKey] = optionsCollection;
    }

    public static void AddMsalAcquireTokenConfiguration<TBuilder>(
        this HttpRequestMessage request,
        Action<TBuilder> configureBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        ConfigureOptions<TBuilder> configureOptions = new(configureBuilder);
        request.AddMsalAcquireTokenConfiguration(configureOptions);
    }

    public static void AddMsalAcquireTokenConfiguration<TBuilder>(
        this HttpRequestMessage request,
        Action<string?, TBuilder> configureBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        ConfigureAllOptions<TBuilder> configureOptions = new(configureBuilder);
        request.AddMsalAcquireTokenConfiguration(configureOptions);
    }

    public static void AddMsalAcquireTokenPostConfiguration<TBuilder>(
        this HttpRequestMessage request,
        IPostConfigureOptions<TBuilder> configureBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        IDictionary<string, object?> options = request.GetOptions();
        string optionsKey = typeof(IPostConfigureOptions<TBuilder>).FullName!;
        options.TryGetValue(optionsKey, out object? optionsExisting);
        ICollection<IPostConfigureOptions<TBuilder>> optionsCollection = optionsExisting switch
        {
            IPostConfigureOptions<TBuilder> optionsExistingSingle => [optionsExistingSingle],
            ICollection<IPostConfigureOptions<TBuilder>> optionsExistingCollection => optionsExistingCollection,
            IEnumerable<IPostConfigureOptions<TBuilder>> optionsExistingEnumerable => [.. optionsExistingEnumerable],
            _ => []
        };
        optionsCollection.Add(configureBuilder);
        options[optionsKey] = optionsCollection;
    }

    public static void AddMsalAcquireTokenPostConfiguration<TBuilder>(
        this HttpRequestMessage request,
        Action<TBuilder> configureBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        PostConfigureOptions<TBuilder> configureOptions = new(name: null, configureBuilder);
        request.AddMsalAcquireTokenPostConfiguration(configureOptions);
    }

    public static void AddMsalAcquireTokenPostConfiguration<TBuilder>(
        this HttpRequestMessage request,
        Action<string?, TBuilder> configureBuilder
        ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
    {
        PostConfigureAllOptions<TBuilder> configureOptions = new(configureBuilder);
        request.AddMsalAcquireTokenPostConfiguration(configureOptions);
    }
}