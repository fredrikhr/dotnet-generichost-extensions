using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.LoggingExtensions;

namespace Microsoft.Identity.Client;

public static class MsalOptionsBuilderExtensions
{
    public static OptionsBuilder<BrokerOptions> SetConstructorArguments(
        this OptionsBuilder<BrokerOptions> optionsBuilder,
        BrokerOptions.OperatingSystems enabledOn
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        optionsBuilder.Services.AddSingleton(new BrokerOptionsParameters(enabledOn));
        optionsBuilder.Services.AddTransient<
            IOptionsFactory<BrokerOptions>,
            BrokerOptionsFactory
            >();
        return optionsBuilder;
    }

    private static OptionsBuilder<TBuilder> ConfigureLoggingImpl<TBuilder, TApplication>(
        this OptionsBuilder<TBuilder> optionsBuilder,
        bool enablePiiLogging = false
        )
        where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        optionsBuilder.Services.AddLogging();
        optionsBuilder.Configure<ILogger<TApplication>>((builder, logger) =>
        {
            IdentityLoggerAdapter msalLogging = new(logger);
            builder.WithLogging(msalLogging, enablePiiLogging);
        });

        return optionsBuilder;
    }

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> ConfigureLogging(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder,
        bool enablePiiLogging = false
        ) => optionsBuilder.ConfigureLoggingImpl
            <ConfidentialClientApplicationBuilder, ConfidentialClientApplication>(
            enablePiiLogging
            );

    public static OptionsBuilder<PublicClientApplicationBuilder> ConfigureLogging(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder,
        bool enablePiiLogging = false
        ) => optionsBuilder.ConfigureLoggingImpl
            <PublicClientApplicationBuilder, PublicClientApplication>(
            enablePiiLogging
            );

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> ConfigureLogging(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder,
        bool enablePiiLogging = false
        ) => optionsBuilder.ConfigureLoggingImpl
            <ManagedIdentityApplicationBuilder, ManagedIdentityApplication>(
            enablePiiLogging
            );

    private static OptionsBuilder<TBuilder> ConfigureMsalHttpFactoryImpl<TBuilder>(this OptionsBuilder<TBuilder> optionsBuilder)
        where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        optionsBuilder.Services.AddHttpClient(optionsBuilder.Name);
        optionsBuilder.Configure<IHttpClientFactory>((builder, httpFactory) =>
        {
            MsalHttpClientFactory msalHttp = new(httpFactory, optionsBuilder.Name);
            builder.WithHttpClientFactory(msalHttp);
        });

        return optionsBuilder;
    }

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> ConfigureMsalHttpFactory(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder
        ) => optionsBuilder.ConfigureMsalHttpFactoryImpl();

    public static OptionsBuilder<PublicClientApplicationBuilder> ConfigureMsalHttpFactory(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder
        ) => optionsBuilder.ConfigureMsalHttpFactoryImpl();

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> ConfigureMsalHttpFactory(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder
        ) => optionsBuilder.ConfigureMsalHttpFactoryImpl();

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> WithInstance(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder,
        Action<OptionsBuilder<IConfidentialClientApplication>> configureInstance
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        if (configureInstance is not null)
            configureInstance(new(optionsBuilder.Services, optionsBuilder.Name));
        return optionsBuilder;
    }

    public static OptionsBuilder<PublicClientApplicationBuilder> WithInstance(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder,
        Action<OptionsBuilder<IPublicClientApplication>> configureInstance
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        if (configureInstance is not null)
            configureInstance(new(optionsBuilder.Services, optionsBuilder.Name));
        return optionsBuilder;
    }

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> WithInstance(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder,
        Action<OptionsBuilder<IManagedIdentityApplication>> configureInstance
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        if (configureInstance is not null)
            configureInstance(new(optionsBuilder.Services, optionsBuilder.Name));
        return optionsBuilder;
    }
}