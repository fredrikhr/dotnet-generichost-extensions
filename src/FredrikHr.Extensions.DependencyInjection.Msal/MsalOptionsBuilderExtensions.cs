using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        optionsBuilder.Services.Configure<BrokerOptionsParameters>(
            optionsBuilder.Name,
            p => p.EnabledOn = enabledOn
            );
        optionsBuilder.Services.TryAddTransient<
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

    private static OptionsBuilder<TBuilder> UseHttpClientFactoryImpl<TBuilder>(
        this OptionsBuilder<TBuilder> optionsBuilder,
        string? name
        )
        where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        optionsBuilder.Services.AddHttpClient(optionsBuilder.Name);
        optionsBuilder.Configure<IHttpMessageHandlerFactory>((builder, httpFactory) =>
        {
            MsalHttpClientFactory msalHttp = new(httpFactory, name);
            builder.WithHttpClientFactory(msalHttp);
        });

        return optionsBuilder;
    }

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> UseHttpClientFactory(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder
        ) => optionsBuilder.UseHttpClientFactoryImpl(optionsBuilder?.Name);

    public static OptionsBuilder<PublicClientApplicationBuilder> UseHttpClientFactory(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder
        ) => optionsBuilder.UseHttpClientFactoryImpl(optionsBuilder?.Name);

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> UseHttpClientFactory(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder
        ) => optionsBuilder.UseHttpClientFactoryImpl(optionsBuilder?.Name);

    public static OptionsBuilder<ConfidentialClientApplicationBuilder> UseHttpClient(
        this OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder,
        string? name = null
        ) => optionsBuilder.UseHttpClientFactoryImpl(name);

    public static OptionsBuilder<PublicClientApplicationBuilder> UseHttpClient(
        this OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder,
        string? name = null
        ) => optionsBuilder.UseHttpClientFactoryImpl(name);

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> UseHttpClient(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder,
        string? name = null
        ) => optionsBuilder.UseHttpClientFactoryImpl(name);

    public static OptionsBuilder<ManagedIdentityApplicationBuilder> UseManagedIdentityId(
        this OptionsBuilder<ManagedIdentityApplicationBuilder> builder,
        AppConfig.ManagedIdentityId managedIdentityId
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(builder);
#else
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
#endif
        builder.Services.Configure<ManagedIdentityApplicationOptions>(
            builder.Name,
            options => options.ManagedIdentityId = managedIdentityId
            );
        return builder;
    }
}