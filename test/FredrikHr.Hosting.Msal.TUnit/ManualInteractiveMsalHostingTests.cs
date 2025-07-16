using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace FredrikHr.Hosting.Msal.TUnit;

[ClassDataSource<TUnitHostBuilderFactory>(Shared = SharedType.PerAssembly)]
public class ManualInteractiveMsalHostingTests(TUnitHostBuilderFactory hostBuilderFactory)
{
    private readonly TUnitHostBuilderFactory _hostBuilderFactory =
        hostBuilderFactory;

    [Test, Explicit]
    public async Task PublicInteractiveUserTokenForOpenId(CancellationToken cancelToken = default)
    {
        HostApplicationBuilder hostBuilder = _hostBuilderFactory.CreateHostAppBuilder();
        hostBuilder.Services.AddOptions<ApplicationOptions>()
            .BindConfiguration(ConfigurationPath.Combine("Microsoft.Identity.Client", "ApplicationOptions"));
        hostBuilder.Services.InheritAll<
            PublicClientApplicationOptions,
            ApplicationOptions
            >();
        hostBuilder.Services.AddOptions<PublicClientApplicationOptions>()
            .BindConfiguration(ConfigurationPath.Combine("Microsoft.Identity.Client", "PublicClient"));
        hostBuilder.Services.AddMsal()
            .UseLogging(enablePiiLogging: hostBuilder.Environment.IsDevelopment())
            .UseHttpClientFactory()
            .UseMsalUserTokenCacheHelper()
            ;
        hostBuilder.Services.AddMsalPersistentCacheHelper();
        if (hostBuilder.Environment.IsDevelopment())
        {
            hostBuilder.Services.ConfigureAll<StorageCreationParameters>((name, storageParams) =>
            {
                Assembly assembly = GetType().Assembly;
                UserSecretsIdAttribute? attribute = assembly.GetCustomAttribute<UserSecretsIdAttribute>();
                if (attribute?.UserSecretsId is not string secretsId) return;
                string secretsPath;
                try
                {
                    secretsPath = PathHelper.GetSecretsPathFromSecretsId(
                        secretsId
                    );
                }
                catch (InvalidOperationException) { return; }
                if (Path.GetDirectoryName(secretsPath) is not string secretsDir) return;
                storageParams.CacheDirectory = secretsDir;
            });
        }

        using IHost host = hostBuilder.Build();
        await host.StartAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        IServiceProvider serviceProvider = host.Services;

        IPublicClientApplication publicMsal = serviceProvider
            .GetRequiredService<IOptions<IPublicClientApplication>>()
            .Value;

        IEnumerable<string> msalScopes = ["openid"];
        AuthenticationResult? msalAuthResult = null;
        try
        {
            IAccount? msalAccount = (await publicMsal.GetAccountsAsync()
                .ConfigureAwait(continueOnCapturedContext: false)
                ).SingleOrDefault();
            if (msalAccount is not null)
            {
                msalAuthResult = await publicMsal.AcquireTokenSilent(
                    msalScopes,
                    msalAccount
                    ).ExecuteAsync(cancelToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }
        catch (MsalClientException)
        {
            msalAuthResult = null;
        }
        if (msalAuthResult is null)
        {
            var msalRequest = publicMsal.AcquireTokenInteractive(msalScopes)
                .WithPrompt(Prompt.SelectAccount)
                ;
            msalAuthResult = await msalRequest.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        
        await host.StopAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }
}