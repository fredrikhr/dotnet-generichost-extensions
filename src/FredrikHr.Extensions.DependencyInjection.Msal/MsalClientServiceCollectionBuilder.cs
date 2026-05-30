using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.LoggingExtensions;

namespace Microsoft.Identity.Client;

public class MsalClientServiceCollectionBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services
        ?? throw new ArgumentNullException(nameof(services));

    public MsalClientServiceCollectionBuilder ConfigureAllBrokerOptions(
        BrokerOptions.OperatingSystems enabledOn,
        Action<string?, BrokerOptions>? configureOptions = null
        ) => ConfigureAllBrokerOptions(_ => enabledOn, configureOptions);

    public MsalClientServiceCollectionBuilder ConfigureAllBrokerOptions(
        Func<string?, BrokerOptions.OperatingSystems> enabledOn,
        Action<string?, BrokerOptions>? configureOptions = null
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(enabledOn);
#else
        _ = enabledOn ?? throw new ArgumentNullException(nameof(enabledOn));
#endif

        Services.AddOptions();
        Services.ConfigureAll<BrokerOptionsParameters>(
            (name, p) => p.EnabledOn = enabledOn(name)
            );
        Services.TryAddTransient<
            IOptionsFactory<BrokerOptions>,
            BrokerOptionsFactory
            >();
        if (configureOptions is not null)
        {
            Services.ConfigureAll(configureOptions);
        }
        return this;
    }

    public MsalClientServiceCollectionBuilder UseLogging(
        bool enablePiiLogging = false
        )
    {
        Services.AddLogging();
        Services.ConfigureAll<PublicClientApplicationBuilder, ILoggerFactory>(
            ConfigureBuilderLogging<PublicClientApplicationBuilder, PublicClientApplication>
            );
        Services.ConfigureAll<ConfidentialClientApplicationBuilder, ILoggerFactory>(
            ConfigureBuilderLogging<ConfidentialClientApplicationBuilder, ConfidentialClientApplication>
            );
        Services.ConfigureAll<ManagedIdentityApplicationBuilder, ILoggerFactory>(
            ConfigureBuilderLogging<ManagedIdentityApplicationBuilder, ManagedIdentityApplication>
            );

        return this;

        static IdentityLoggerAdapter CreateLoggingAdapter<TApplication>(
            string? name,
            ILoggerFactory loggerFactory
            )
        {
            ILogger logger = name switch
            {
                { Length: > 0 } => loggerFactory.CreateLogger(
                    $"{typeof(TApplication).GetType()}.{name}"
                    ),
                _ => loggerFactory.CreateLogger<TApplication>(),
            };
            return new(logger);
        }

        void ConfigureBuilderLogging<TBuilder, TApplication>(
            string? name,
            TBuilder builder,
            ILoggerFactory loggerFactory
            ) where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
        {
            IdentityLoggerAdapter msalLogging = CreateLoggingAdapter<TApplication>(name, loggerFactory);
            builder.WithLogging(msalLogging, enablePiiLogging);
        }
    }

    public MsalClientServiceCollectionBuilder UseHttpClientFactory()
    {
        Services.AddHttpClient();
        Services.ConfigureAll<PublicClientApplicationBuilder, IHttpMessageHandlerFactory>(
            ConfigureBuilderHttpClientFactory<PublicClientApplicationBuilder, PublicClientApplication>
            );
        Services.ConfigureAll<ConfidentialClientApplicationBuilder, IHttpMessageHandlerFactory>(
            ConfigureBuilderHttpClientFactory<ConfidentialClientApplicationBuilder, ConfidentialClientApplication>
            );
        Services.ConfigureAll<ManagedIdentityApplicationBuilder, IHttpMessageHandlerFactory>(
            ConfigureBuilderHttpClientFactory<ManagedIdentityApplicationBuilder, ManagedIdentityApplication>
            );

        return this;

        static void ConfigureBuilderHttpClientFactory<TBuilder, TApplication>(
            string? name,
            TBuilder builder,
            IHttpMessageHandlerFactory httpFactory
            ) where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
        {
            MsalHttpClientFactory msalHttpFactory = new(httpFactory, name);
            builder.WithHttpClientFactory(msalHttpFactory);
        }
    }

    public MsalClientServiceCollectionBuilder ConfigureAllApplicationOptions(
        Action<ApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllApplicationOptions(
        Action<string?, ApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllApplicationOptions(
        Action<ApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllApplicationOptions(
        Action<string?, ApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllPublicClientApplicationOptions(
        Action<PublicClientApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllPublicClientApplicationOptions(
        Action<string?, PublicClientApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllPublicClientApplicationOptions(
        Action<PublicClientApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllPublicClientApplicationOptions(
        Action<string?, PublicClientApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllConfidentialClientApplicationOptions(
        Action<ConfidentialClientApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllConfidentialClientApplicationOptions(
        Action<string?, ConfidentialClientApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllConfidentialClientApplicationOptions(
        Action<ConfidentialClientApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllConfidentialClientApplicationOptions(
        Action<string?, ConfidentialClientApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllManagedIdentityApplicationOptions(
        Action<ManagedIdentityApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllManagedIdentityApplicationOptions(
        Action<string?, ManagedIdentityApplicationOptions> configureOptions
        )
    {
        Services.ConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllManagedIdentityApplicationOptions(
        Action<ManagedIdentityApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllManagedIdentityApplicationOptions(
        Action<string?, ManagedIdentityApplicationOptions> configureOptions
        )
    {
        Services.PostConfigureAll(configureOptions);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllPublicClientApplications(
        Action<string?, PublicClientApplicationBuilder> configureBuilders
        )
    {
        Services.ConfigureAll(configureBuilders);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllConfidentialClientApplications(
        Action<string?, ConfidentialClientApplicationBuilder> configureBuilders
        )
    {
        Services.ConfigureAll(configureBuilders);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureAllManagedIdentityApplications(
        Action<string?, ManagedIdentityApplicationBuilder> configureBuilders
        )
    {
        Services.ConfigureAll(configureBuilders);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllPublicClientApplications(
        Action<string?, PublicClientApplicationBuilder> configureBuilders
        )
    {
        Services.PostConfigureAll(configureBuilders);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllConfidentialClientApplications(
        Action<string?, ConfidentialClientApplicationBuilder> configureBuilders
        )
    {
        Services.PostConfigureAll(configureBuilders);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureAllManagedIdentityApplications(
        Action<string?, ManagedIdentityApplicationBuilder> configureBuilders
        )
    {
        Services.PostConfigureAll(configureBuilders);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigurePublicClientApplication(
        string? name,
        Action<PublicClientApplicationBuilder> configureBuilder
        )
    {
        Services.Configure(name, configureBuilder);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureConfidentialClientApplication(
        string? name,
        Action<ConfidentialClientApplicationBuilder> configureBuilder
        )
    {
        Services.Configure(name, configureBuilder);
        return this;
    }

    public MsalClientServiceCollectionBuilder ConfigureManagedIdentityApplication(
        string? name,
        Action<ManagedIdentityApplicationBuilder> configureBuilder
        )
    {
        Services.Configure(name, configureBuilder);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigurePublicClientApplication(
        string? name,
        Action<PublicClientApplicationBuilder> configureBuilder
        )
    {
        Services.PostConfigure(name, configureBuilder);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureConfidentialClientApplication(
        string? name,
        Action<ConfidentialClientApplicationBuilder> configureBuilder
        )
    {
        Services.PostConfigure(name, configureBuilder);
        return this;
    }

    public MsalClientServiceCollectionBuilder PostConfigureManagedIdentityApplication(
        string? name,
        Action<ManagedIdentityApplicationBuilder> configureBuilder
        )
    {
        Services.PostConfigure(name, configureBuilder);
        return this;
    }
}