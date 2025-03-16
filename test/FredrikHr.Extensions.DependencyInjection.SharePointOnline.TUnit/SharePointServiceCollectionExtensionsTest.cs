using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;

using TUnit.Assertions.AssertConditions.Throws;

namespace FredrikHr.Extensions.DependencyInjection.SharePointOnline.TUnit;

public class SharePointServiceCollectionExtensionsTest
{
    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0053: Use expression body for lambda expression"
        )]
    public async Task AddClientContextThrowsIfServiceCollectionNull()
    {
        ServiceCollection services = null!;

        await Assert
            .That(() =>
            {
                services.AddClientContextFactory();
            })
            .Throws<ArgumentNullException>()
            .WithParameterName(nameof(services))
            ;
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0053: Use expression body for lambda expression"
        )]
    public async Task AddClientContextAddsSingleton()
    {
        ServiceCollection services = new();
        services.AddClientContextFactory(useHttpClientFactory: false);

        await Assert
            .That(services)
            .Contains(desc =>
            {
                return desc.Lifetime == ServiceLifetime.Singleton &&
                    desc.ServiceType == typeof(ClientContextFactory)
                    ;
            }).And.Contains(desc =>
            {
                return desc.Lifetime == ServiceLifetime.Singleton &&
                    desc.ServiceType == typeof(IOptionsFactory<ClientContext>) &&
                    desc.ImplementationType == typeof(ClientContextFactory)
                    ;
            });
    }

    [Test]
    public async Task ClientContextFactoryCreateFailsWithoutWebUrl()
    {
        ServiceCollection services = new();
        services.AddClientContextFactory(useHttpClientFactory: false);
        using var serviceProvider = services.BuildServiceProvider();
        ClientContextFactory factory = serviceProvider
            .GetRequiredService<ClientContextFactory>();
        await Assert.That(() => factory.Create(Options.DefaultName))
            .Throws<ArgumentNullException>()
            .WithParameterName("webFullUrl");
    }

    [Test]
    public async Task ClientContextFactoryUsesWebUrlOptions()
    {
        const string webUrl = "https://example.sharepoint.com";
        ServiceCollection services = new();
        services.AddOptions<ClientContextConstructorOptions>()
            .Configure(o => o.WebUrl = webUrl);
        services.AddClientContextFactory(useHttpClientFactory: false);
        using var serviceProvider = services.BuildServiceProvider();
        ClientContextFactory factory = serviceProvider
            .GetRequiredService<ClientContextFactory>();

        using var context = factory.Create();

        await Assert.That(context.Url).IsEqualTo(webUrl);
    }

    [Test]
    public async Task ClientContextFactoryUsesWebUrlParameters()
    {
        const string webUrl = "https://example.sharepoint.com";
        ServiceCollection services = new();
        services.AddClientContextFactory(useHttpClientFactory: false);
        using var serviceProvider = services.BuildServiceProvider();
        ClientContextFactory factory = serviceProvider
            .GetRequiredService<ClientContextFactory>();

        using var context = factory.CreateWithWebUrl(webUrl);

        await Assert.That(context.Url).IsEqualTo(webUrl);
    }

    [Test]
    public async Task ClientContextResolvesAsIOptions()
    {
        const string webUrl = "https://example.sharepoint.com";
        ServiceCollection services = new();
        services.AddOptions<ClientContextConstructorOptions>()
            .Configure(o => o.WebUrl = webUrl);
        services.AddClientContextFactory(useHttpClientFactory: false);
        using var serviceProvider = services.BuildServiceProvider();

        using var context = serviceProvider
            .GetRequiredService<IOptions<ClientContext>>()
            .Value;

        await Assert.That(context.Url).IsEqualTo(webUrl);
    }

    [Test]
    public async Task AddClientContextUsingHttpClientSetWebRequestExecutorFactory()
    {
        const string webUrl = "https://example.sharepoint.com";
        ServiceCollection services = new();
        services.AddClientContextFactory(useHttpClientFactory: true);
        using var serviceProvider = services.BuildServiceProvider();
        ClientContextFactory factory = serviceProvider
            .GetRequiredService<ClientContextFactory>();

        using var context = factory.CreateWithWebUrl(webUrl);

        await Assert.That(context.WebRequestExecutorFactory)
            .IsTypeOf<HttpClientWebRequestExecutorFactory>();
    }

    [Test]
    public async Task ClientContextFactoryUsesConfigureOptions()
    {
        const string webUrl = "https://example.sharepoint.com";
        bool configureContextHasRun = false;
        ServiceCollection services = new();
        services.AddClientContextFactory(useHttpClientFactory: false);
        services.ConfigureAll<ClientContext>(ctx => configureContextHasRun = true);
        using var serviceProvider = services.BuildServiceProvider();
        ClientContextFactory factory = serviceProvider
            .GetRequiredService<ClientContextFactory>();

        using var context = factory.CreateWithWebUrl(webUrl);

        await Assert.That(configureContextHasRun).IsTrue();
    }
}
