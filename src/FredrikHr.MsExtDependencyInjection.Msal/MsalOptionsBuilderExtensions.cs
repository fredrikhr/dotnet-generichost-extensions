using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.LoggingExtensions;

namespace Microsoft.Identity.Client;

public static class MsalOptionsBuilderExtensions
{
    public static OptionsBuilder<BrokerOptions> UseConstructorArguments(
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

    private static OptionsBuilder<TBuilder> UseLoggingImpl<TBuilder, TApplication>(
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

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> UseLogging(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder,
        bool enablePiiLogging = false
        ) => optionsBuilder.UseLoggingImpl
            <ConfidentialClientApplicationBuilder, ConfidentialClientApplication>(
            enablePiiLogging
            );

    public static OptionsBuilder<PublicClientApplicationBuilder> UseLogging(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder,
        bool enablePiiLogging = false
        ) => optionsBuilder.UseLoggingImpl
            <PublicClientApplicationBuilder, PublicClientApplication>(
            enablePiiLogging
            );

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> UseLogging(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder,
        bool enablePiiLogging = false
        ) => optionsBuilder.UseLoggingImpl
            <ManagedIdentityApplicationBuilder, ManagedIdentityApplication>(
            enablePiiLogging
            );

    private static OptionsBuilder<TBuilder> UseHttpClientFactoryImpl<TBuilder>(this OptionsBuilder<TBuilder> optionsBuilder)
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

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> UseHttpClientFactory(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder
        ) => optionsBuilder.UseHttpClientFactoryImpl();

    public static OptionsBuilder<PublicClientApplicationBuilder> UseHttpClientFactory(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder
        ) => optionsBuilder.UseHttpClientFactoryImpl();

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> UseHttpClientFactory(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder
        ) => optionsBuilder.UseHttpClientFactoryImpl();

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> ConfigureInstance(
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

    public static OptionsBuilder<PublicClientApplicationBuilder> ConfigureInstance(
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

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> ConfigureInstance(
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