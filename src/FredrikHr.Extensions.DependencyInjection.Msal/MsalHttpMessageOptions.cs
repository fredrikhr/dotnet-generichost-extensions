using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public static class MsalHttpMessageOptions
{
    public const string PublicClientApplicationOptionsKey = $"Microsoft.Identity.Client.{nameof(PublicClientApplication)}";
    public const string ConfidentialClientApplicationOptionsKey = $"Microsoft.Identity.Client.{nameof(ConfidentialClientApplication)}";
    public const string ManagedIdentityApplicationOptionsKey = $"Microsoft.Identity.Client.{nameof(ManagedIdentityApplication)}";
    public const string ServiceScopeOptionsKey = "Microsoft.Identity.Client.ServiceScope";
    public const string ServiceProviderOptionsKey = "Microsoft.Identity.Client.ServiceProvider";
    public const string ResourceOptionsKey = "Microsoft.Identity.Client.Resource";
    public const string PermissionScopesOptionsKey = "Microsoft.Identity.Client.PermissionScopes";
    public const string ScopesOptionsKey = "Microsoft.Identity.Client.Scopes";
    public const string LoginHintOptionsKey = "Microsoft.Identity.Client.LoginHint";

    extension(HttpRequestMessage request)
    {
        internal IDictionary<string, object?> GetOptions()
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

        internal bool TryGetOptionByType<T>(out T? optionValue)
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

        internal IEnumerable<T> GetOptionEnumerableByType<T>()
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

        internal void SetOptionByType<T>(T? optionValue)
        {
            IDictionary<string, object?> options = request.GetOptions();
            string optionKey = typeof(T).FullName!;
            options[optionKey] = optionValue;
        }

        internal void AddOptionByType<T>(T? optionValue)
        {
            IDictionary<string, object?> options = request.GetOptions();
            string optionKey = typeof(T).FullName!;
            options.Add(optionKey, optionValue);
        }

        public void SetMsalPublicClient(IPublicClientApplication client)
            => request.GetOptions()[PublicClientApplicationOptionsKey] = client;

        public void SetMsalConfidentialClient(IConfidentialClientApplication client)
            => request.GetOptions()[ConfidentialClientApplicationOptionsKey] = client;

        public void SetMsalManagedIdentityClient(IManagedIdentityApplication client)
            => request.GetOptions()[ManagedIdentityApplicationOptionsKey] = client;

        public void SetMsalServiceScope(IServiceScope serviceScope)
            => request.GetOptions()[ServiceScopeOptionsKey] = serviceScope;

        public IServiceScope? GetMsalServiceScope()
            => request.GetOptions().TryGetValue(
                ServiceScopeOptionsKey,
                out object? optionObjectValue
                )
                ? optionObjectValue as IServiceScope
                : null;

        public void SetMsalServiceProvider(IServiceProvider serviceProvider)
            => request.GetOptions()[ServiceProviderOptionsKey] = serviceProvider;

        public IServiceProvider? GetMsalServiceProvider(
            )
        {
            return
                request.GetMsalServiceScope() is { ServiceProvider: IServiceProvider scopeServiceProvider }
                ? scopeServiceProvider
                : request.GetOptions().TryGetValue(
                    ServiceProviderOptionsKey,
                    out object? optionObjectValue
                )
                ? optionObjectValue as IServiceProvider
                : null;
        }

        public void SetMsalAccount(IAccount account)
            => request.SetOptionByType(account);

        public IAccount? GetMsalAccount() => request.TryGetOptionByType(
            out IAccount? account
            ) ? account : null;

        public void SetMsalResource(string resource)
            => request.GetOptions()[ResourceOptionsKey] = resource;

        public string? GetMsalResource() => request.GetOptions().TryGetValue(
            ResourceOptionsKey,
            out object? optionObjectValue
            ) ? optionObjectValue as string : null;

        public void SetMsalPermissionScopes(IEnumerable<string> scopes)
            => request.GetOptions()[PermissionScopesOptionsKey] = scopes;

        public void AddMsalPermissionScopes(params IEnumerable<string> scopes)
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
            else if (scopes is string[] scopesArray)
            {
                foreach (string scope in scopesArray)
                {
                    combinedScopes.Add(scope);
                }
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

        public void AddMsalPermissionScopes(params ReadOnlySpan<string> scopes)
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

        public IEnumerable<string> GetMsalPermissionScopes()
        {
            return request.GetOptions().TryGetValue(
                PermissionScopesOptionsKey,
                out object? optionsObjectValue
                ) ? optionsObjectValue switch
                {
                    IEnumerable<string> optionsEnumerable => optionsEnumerable,
                    string optionsSingleValue => [optionsSingleValue],
                    _ => [],
                } : [];
        }

        public void SetMsalScopes(IEnumerable<string> scopes)
            => request.GetOptions()[ScopesOptionsKey] = scopes;

        public void AddMsalScopes(params IEnumerable<string> scopes)
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
            else if (scopes is string[] scopesArray)
            {
                foreach (string scope in scopesArray)
                {
                    combinedScopes.Add(scope);
                }
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

        public void AddMsalScopes(params ReadOnlySpan<string> scopes)
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

        public IEnumerable<string> GetMsalScopes()
        {
            return request.GetOptions().TryGetValue(ScopesOptionsKey, out object? optionsObjectValue)
                ? optionsObjectValue switch
                {
                    IEnumerable<string> optionsEnumerable => optionsEnumerable,
                    string optionsSingleValue => [optionsSingleValue],
                    _ => [],
                } : [];
        }

        public void SetMsalLoginHint(string loginHint)
            => request.GetOptions()[LoginHintOptionsKey] = loginHint;

        public string? GetMsalLoginHint()
        {
            string? optionsValue = request.GetOptions()
                .TryGetValue(LoginHintOptionsKey, out object? optionObjectValue)
                ? optionObjectValue as string
                : null;
            return optionsValue;
        }

        public void AddMsalAcquireTokenConfiguration<TBuilder>(
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

        public void AddMsalAcquireTokenConfiguration<TBuilder>(
            Action<TBuilder> configureBuilder
            ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
        {
            ConfigureOptions<TBuilder> configureOptions = new(configureBuilder);
            request.AddMsalAcquireTokenConfiguration(configureOptions);
        }

        public void AddMsalAcquireTokenConfiguration<TBuilder>(
            Action<string?, TBuilder> configureBuilder
            ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
        {
            ConfigureAllOptions<TBuilder> configureOptions = new(configureBuilder);
            request.AddMsalAcquireTokenConfiguration(configureOptions);
        }

        public void AddMsalAcquireTokenPostConfiguration<TBuilder>(
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

        public void AddMsalAcquireTokenPostConfiguration<TBuilder>(
            Action<TBuilder> configureBuilder
            ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
        {
            PostConfigureOptions<TBuilder> configureOptions = new(name: null, configureBuilder);
            request.AddMsalAcquireTokenPostConfiguration(configureOptions);
        }

        public void AddMsalAcquireTokenPostConfiguration<TBuilder>(
            Action<string?, TBuilder> configureBuilder
            ) where TBuilder : BaseAbstractAcquireTokenParameterBuilder<TBuilder>
        {
            PostConfigureAllOptions<TBuilder> configureOptions = new(configureBuilder);
            request.AddMsalAcquireTokenPostConfiguration(configureOptions);
        }
    }

    extension(HttpResponseMessage response)
    {
        public IAccount? GetMsalAccount()
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(response);
#else
            _ = response ?? throw new ArgumentNullException(nameof(response));
#endif
            return GetMsalAccount(response.RequestMessage!);
        }

        public AuthenticationResult? GetMsalAuthenticationResult()
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(response);
#else
            _ = response ?? throw new ArgumentNullException(nameof(response));
#endif
            return response.RequestMessage!
                .TryGetOptionByType(out AuthenticationResult? result)
                ? result
                : null;
        }
    }
}