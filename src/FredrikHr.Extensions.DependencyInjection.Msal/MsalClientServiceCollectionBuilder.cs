using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.LoggingExtensions;

namespace Microsoft.Identity.Client;

public class MsalClientServiceCollectionBuilder
{
    public IServiceCollection Services { get; }

    public MsalClientServiceCollectionBuilder(IServiceCollection services)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
#else
        Services = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddOptions();
        services.TryAddSingleton<
            IOptionsFactory<PublicClientApplicationBuilder>,
            PublicClientApplicationBuilderFactory
            >();
        services.TryAddSingleton<
            IOptionsFactory<IPublicClientApplication>,
            PublicClientApplicationFactory
            >();

        services.TryAddSingleton<
            IOptionsFactory<ConfidentialClientApplicationBuilder>,
            ConfidentialClientApplicationBuilderFactory
            >();
        services.TryAddSingleton<
            IOptionsFactory<IConfidentialClientApplication>,
            ConfidentialClientApplicationFactory
            >();

        services.TryAddSingleton(AppConfig.ManagedIdentityId.SystemAssigned);
        services.TryAddSingleton<
            IOptionsFactory<ManagedIdentityApplicationBuilder>,
            ManagedIdentityApplicationBuilderFactory
            >();
        services.TryAddSingleton<
            IOptionsFactory<IManagedIdentityApplication>,
            ManagedIdentityApplicationFactory
            >();
    }

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

    public MsalClientServiceCollectionBuilder WithPublicClient(
        Action<OptionsBuilder<PublicClientApplicationBuilder>> configureBuilder,
        string? name = null
        )
    {
        if (configureBuilder is not null)
        {
            OptionsBuilder<PublicClientApplicationBuilder> builder =
                new(Services, name);
            configureBuilder(builder);
        }
        return this;
    }

    public MsalClientServiceCollectionBuilder WithConfidentialClient(
        Action<OptionsBuilder<ConfidentialClientApplicationBuilder>> configureBuilder,
        string? name = null
        )
    {
        if (configureBuilder is not null)
        {
            OptionsBuilder<ConfidentialClientApplicationBuilder> builder =
                new(Services, name);
            configureBuilder(builder);
        }
        return this;
    }

    public MsalClientServiceCollectionBuilder WithManagedIdentityClient(
        Action<OptionsBuilder<ManagedIdentityApplicationBuilder>> configureBuilder,
        string? name = null
        )
    {
        if (configureBuilder is not null)
        {
            OptionsBuilder<ManagedIdentityApplicationBuilder> builder =
                new(Services, name);
            configureBuilder(builder);
        }
        return this;
    }
}