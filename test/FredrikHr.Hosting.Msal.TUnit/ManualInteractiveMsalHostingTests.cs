using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var hostBuilder = _hostBuilderFactory.CreateHostAppBuilder();
        hostBuilder.Services.AddOptions<ApplicationOptions>()
            .BindConfiguration(ConfigurationPath.Combine("Microsoft.Identity.Client", "ApplicationOptions"));
        hostBuilder.Services.AddOptions<PublicClientApplicationOptions>()
            .ConfigureApplyBaseType<PublicClientApplicationOptions, ApplicationOptions>()
            .BindConfiguration(ConfigurationPath.Combine("Microsoft.Identity.Client", "PublicClient"));
        hostBuilder.Services.AddMsalPublicClient()
            .UseLogging()
            .UseHttpClientFactory()
            ;

        using var host = hostBuilder.Build();
        await host.StartAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        var serviceProvider = host.Services;

        var publicMsal = serviceProvider
            .GetRequiredService<IPublicClientApplication>();

        var msalRequest = publicMsal.AcquireTokenInteractive(["openid"])
            .WithPrompt(Prompt.SelectAccount)
            ;
        var msalAuthResult = await msalRequest.ExecuteAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        await host.StopAsync(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }
}