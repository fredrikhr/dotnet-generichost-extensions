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

        services.AddSingleton<
            IOptionsFactory<PublicClientApplicationBuilder>,
            PublicClientApplicationBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<IPublicClientApplication>,
            PublicClientApplicationFactory
            >();

        services.AddSingleton<
            IOptionsFactory<ConfidentialClientApplicationBuilder>,
            ConfidentialClientApplicationBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<IConfidentialClientApplication>,
            ConfidentialClientApplicationFactory
            >();

        services.TryAddSingleton(AppConfig.ManagedIdentityId.SystemAssigned);
        services.AddSingleton<
            IOptionsFactory<ManagedIdentityApplicationBuilder>,
            ManagedIdentityApplicationBuilderFactory
            >();
        services.AddSingleton<
            IOptionsFactory<IManagedIdentityApplication>,
            ManagedIdentityApplicationFactory
            >();
    }

    public MsalClientServiceCollectionBuilder UseLogging(
        bool enablePiiLogging = false
        )
    {
        Services.ConfigureAllNamed<PublicClientApplicationBuilder, ILoggerFactory>(
            enablePiiLogging
            ? ConfigureBuilderPiiLogging<PublicClientApplicationBuilder, PublicClientApplication>
            : ConfigureBuilderLogging<PublicClientApplicationBuilder, PublicClientApplication>
            );
        Services.ConfigureAllNamed<ConfidentialClientApplicationBuilder, ILoggerFactory>(
            enablePiiLogging
            ? ConfigureBuilderPiiLogging<ConfidentialClientApplicationBuilder, ConfidentialClientApplication>
            : ConfigureBuilderLogging<ConfidentialClientApplicationBuilder, ConfidentialClientApplication>
            );
        Services.ConfigureAllNamed<ManagedIdentityApplicationBuilder, ILoggerFactory>(
            enablePiiLogging
            ? ConfigureBuilderPiiLogging<ManagedIdentityApplicationBuilder, ManagedIdentityApplication>
            : ConfigureBuilderLogging<ManagedIdentityApplicationBuilder, ManagedIdentityApplication>
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

        static void ConfigureBuilderLogging<TBuilder, TApplication>(
            string? name,
            ILoggerFactory loggerFactory,
            TBuilder builder
            ) where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
        {
            var msalLogging = CreateLoggingAdapter<TApplication>(name, loggerFactory);
            builder.WithLogging(msalLogging, enablePiiLogging: false);
        }

        static void ConfigureBuilderPiiLogging<TBuilder, TApplication>(
            string? name,
            ILoggerFactory loggerFactory,
            TBuilder builder
            ) where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
        {
            var msalLogging = CreateLoggingAdapter<TApplication>(name, loggerFactory);
            builder.WithLogging(msalLogging, enablePiiLogging: false);
        }
    }

    public MsalClientServiceCollectionBuilder UseHttpClientFactory()
    {
        Services.ConfigureAllNamed<PublicClientApplicationBuilder, IHttpClientFactory>(
            ConfigureBuilderHttpClientFactory<PublicClientApplicationBuilder, PublicClientApplication>
            );
        Services.ConfigureAllNamed<ConfidentialClientApplicationBuilder, IHttpClientFactory>(
            ConfigureBuilderHttpClientFactory<ConfidentialClientApplicationBuilder, ConfidentialClientApplication>
            );
        Services.ConfigureAllNamed<ManagedIdentityApplicationBuilder, IHttpClientFactory>(
            ConfigureBuilderHttpClientFactory<ManagedIdentityApplicationBuilder, ManagedIdentityApplication>
            );

        return this;

        static void ConfigureBuilderHttpClientFactory<TBuilder, TApplication>(
            string? name,
            IHttpClientFactory httpFactory,
            TBuilder builder
            ) where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
        {
            MsalHttpClientFactory msalHttpFactory = new(httpFactory, name);
            builder.WithHttpClientFactory(msalHttpFactory);
        }
    }

    public OptionsBuilder<PublicClientApplicationBuilder> AddPublicClient(
        string? name = null
        ) => new(Services, name);

    public OptionsBuilder<ConfidentialClientApplicationBuilder> AddConfidentialClient(
        string? name = null
        ) => new(Services, name);

    public OptionsBuilder<ManagedIdentityApplicationBuilder> AddManagedIdentityClient(
        string? name = null
        ) => new(Services, name);
}