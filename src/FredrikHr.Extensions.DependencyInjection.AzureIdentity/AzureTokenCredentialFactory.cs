using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

#if NET8_0_OR_GREATER
using System.Threading;
#else
using Lock = object;
#endif

namespace Azure.Identity;

public sealed class AzureTokenCredentialFactory(
    IServiceProvider serviceProvider
    ) : IOptionsFactory<DefaultAzureCredential>,
    IOptionsFactory<AzureCliCredential>,
    IOptionsFactory<AzureDeveloperCliCredential>,
    IOptionsFactory<AzurePowerShellCredential>,
    IOptionsFactory<DeviceCodeCredential>,
    IOptionsFactory<EnvironmentCredential>,
    IOptionsFactory<InteractiveBrowserCredential>,
    IOptionsFactory<SharedTokenCacheCredential>,
    IOptionsFactory<VisualStudioCodeCredential>,
    IOptionsFactory<VisualStudioCredential>,
    IOptionsFactory<WorkloadIdentityCredential>
{
    private IServiceProvider? ServiceProvider { get; } = serviceProvider;

    public AuthorizationCodeCredential CreateAuthorizationCodeCredential(
        string tenantId,
        string clientId,
        string clientSecret,
        string authorizationCode,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<AuthorizationCodeCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(tenantId, clientId, clientSecret, authorizationCode, options)
            : new(tenantId, clientId, clientSecret, authorizationCode);
    }

    public AzureCliCredential CreateAzureCliCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<AzureCliCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public AzureDeveloperCliCredential CreateAzureDeveloperCliCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<AzureDeveloperCliCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public AzurePipelinesCredential CreateAzurePipelinesCredential(
        string tenantId,
        string clientId,
        string serviceConnectionId,
        string systemAccessToken,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<AzurePipelinesCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, serviceConnectionId, systemAccessToken, options);
    }

    public AzurePowerShellCredential CreateAzurePowerShellCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<AzurePowerShellCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public ClientAssertionCredential CreateClientAssertionCredential(
        string tenantId,
        string clientId,
        Func<CancellationToken, Task<string>> assertionCallback,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<ClientAssertionCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, assertionCallback, options);
    }

    public ClientAssertionCredential CreateClientAssertionCredential(
        string tenantId,
        string clientId,
        Func<string> assertionCallback,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<ClientAssertionCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, assertionCallback, options);
    }

    public ClientCertificateCredential CreateClientCertificateCredential(
        string tenantId,
        string clientId,
        string clientCertificatePath,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<ClientCertificateCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientCertificatePath, options);
    }

    public ClientCertificateCredential CreateClientCertificateCredential(
        string tenantId,
        string clientId,
        X509Certificate2 clientCertificate,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<ClientCertificateCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientCertificate, options);
    }

    public ClientSecretCredential CreateClientSecretCredential(
        string tenantId,
        string clientId,
        string clientSecret,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<ClientSecretCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientSecret, options);
    }

    public DefaultAzureCredential CreateDefaultAzureCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<DefaultAzureCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public DeviceCodeCredential CreateDeviceCodeCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<DeviceCodeCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public EnvironmentCredential CreateEnvironmentCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<EnvironmentCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public InteractiveBrowserCredential CreateInteractiveBrowserCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<InteractiveBrowserCredentialOptions>
            >()?.Get(name);
        return options is not null
            ? new(options)
            : new();
    }

    public ManagedIdentityCredential CreateManagedIdentityCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<ManagedIdentityCredentialOptions>
            >()?.Get(name);
        return new(options);
    }

    public OnBehalfOfCredential CreateOnBehalfOfCredential(
        string tenantId,
        string clientId,
        X509Certificate2 clientCertificate,
        string userAssertion,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<OnBehalfOfCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientCertificate, userAssertion, options);
    }

    public OnBehalfOfCredential CreateOnBehalfOfCredential(
        string tenantId,
        string clientId,
        string clientSecret,
        string userAssertion,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<OnBehalfOfCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientSecret, userAssertion, options);
    }

    public OnBehalfOfCredential CreateOnBehalfOfCredential(
        string tenantId,
        string clientId,
        Func<CancellationToken, Task<string>> clientAssertionCallback,
        string userAssertion,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<OnBehalfOfCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientAssertionCallback, userAssertion, options);
    }

    public OnBehalfOfCredential CreateOnBehalfOfCredential(
        string tenantId,
        string clientId,
        Func<string> clientAssertionCallback,
        string userAssertion,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<OnBehalfOfCredentialOptions>
            >()?.Get(name);
        return new(tenantId, clientId, clientAssertionCallback, userAssertion, options);
    }

    public SharedTokenCacheCredential CreateSharedTokenCacheCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<SharedTokenCacheCredentialOptions>
            >()?.Get(name);
        return new(options);
    }

    public UsernamePasswordCredential CreateUsernamePasswordCredential(
        string username,
        string password,
        string tenantId,
        string clientId,
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<UsernamePasswordCredentialOptions>
            >()?.Get(name);
        return new(username, password, tenantId, clientId, options);
    }

    public VisualStudioCodeCredential CreateVisualStudioCodeCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<VisualStudioCodeCredentialOptions>
            >()?.Get(name);
        return new(options);
    }

    public VisualStudioCredential CreateVisualStudioCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<VisualStudioCredentialOptions>
            >()?.Get(name);
        return new(options);
    }

    public WorkloadIdentityCredential CreateWorkloadIdentityCredential(
        string? name = default
        )
    {
        var options = ServiceProvider?.GetRequiredService<
            IOptionsMonitor<WorkloadIdentityCredentialOptions>
            >()?.Get(name);
        return new(options);
    }

    DefaultAzureCredential IOptionsFactory<DefaultAzureCredential>.Create(
        string name
        ) => CreateDefaultAzureCredential(name);

    AzureCliCredential IOptionsFactory<AzureCliCredential>.Create(
        string name
        ) => CreateAzureCliCredential(name);

    AzureDeveloperCliCredential IOptionsFactory<AzureDeveloperCliCredential>.Create(
        string name
        ) => CreateAzureDeveloperCliCredential(name);

    AzurePowerShellCredential IOptionsFactory<AzurePowerShellCredential>.Create(
        string name
        ) => CreateAzurePowerShellCredential(name);

    DeviceCodeCredential IOptionsFactory<DeviceCodeCredential>.Create(
        string name
        ) => CreateDeviceCodeCredential(name);

    EnvironmentCredential IOptionsFactory<EnvironmentCredential>.Create(
        string name
        ) => CreateEnvironmentCredential(name);

    InteractiveBrowserCredential IOptionsFactory<InteractiveBrowserCredential>.Create(
        string name
        ) => CreateInteractiveBrowserCredential(name);

    SharedTokenCacheCredential IOptionsFactory<SharedTokenCacheCredential>.Create(
        string name
        ) => CreateSharedTokenCacheCredential(name);

    VisualStudioCodeCredential IOptionsFactory<VisualStudioCodeCredential>.Create(
        string name
        ) => CreateVisualStudioCodeCredential(name);

    VisualStudioCredential IOptionsFactory<VisualStudioCredential>.Create(
        string name
        ) => CreateVisualStudioCredential(name);

    WorkloadIdentityCredential IOptionsFactory<WorkloadIdentityCredential>.Create(
        string name
        ) => CreateWorkloadIdentityCredential(name);
}
