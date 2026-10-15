using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.LoggingExtensions;

namespace Microsoft.Identity.Client;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1708: Identifiers should differ by more than case",
    Justification = "extension block")]
public static class MsalOptionsBuilder
{
    extension(OptionsBuilder<BrokerOptions> optionsBuilder)
    {
        public OptionsBuilder<BrokerOptions> UseConstructorArguments(
           BrokerOptions.OperatingSystems enabledOn)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
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
    }

    extension<TBuilder>(OptionsBuilder<TBuilder> optionsBuilder)
        where TBuilder : BaseAbstractApplicationBuilder<TBuilder>
    {
        private OptionsBuilder<TBuilder> UseLoggingImpl<TApplication>(
            bool enablePiiLogging = false)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            optionsBuilder.Services.AddLogging();
            optionsBuilder.Configure<ILogger<TApplication>>((builder, logger) =>
            {
                IdentityLoggerAdapter msalLogging = new(logger);
                builder.WithLogging(msalLogging, enablePiiLogging);
            });

            return optionsBuilder;
        }

        private OptionsBuilder<TBuilder> UseHttpClientFactoryImpl(
            string? name)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            optionsBuilder.Services.AddHttpClient(optionsBuilder.Name);
            optionsBuilder.Configure<IHttpClientFactory>((builder, httpFactory) =>
            {
                MsalHttpClientFactory msalHttp = new(httpFactory, name);
                builder.WithHttpClientFactory(msalHttp);
            });

            return optionsBuilder;
        }
    }

    extension(OptionsBuilder<ConfidentialClientApplicationBuilder> optionsBuilder)
    {
        public OptionsBuilder<ConfidentialClientApplicationBuilder> UseLogging(
           bool enablePiiLogging = false)
            => optionsBuilder.UseLoggingImpl<
                ConfidentialClientApplicationBuilder,
                ConfidentialClientApplication
                >(enablePiiLogging);

        public OptionsBuilder<ConfidentialClientApplicationBuilder> UseHttpClientFactory()
            => optionsBuilder.UseHttpClientFactoryImpl(optionsBuilder?.Name);

        public OptionsBuilder<ConfidentialClientApplicationBuilder> UseHttpClient(
            string? name = null)
            => optionsBuilder.UseHttpClientFactoryImpl(name);
    }

    extension(OptionsBuilder<PublicClientApplicationBuilder> optionsBuilder)
    {
        public OptionsBuilder<PublicClientApplicationBuilder> UseLogging(
            bool enablePiiLogging = false)
            => optionsBuilder.UseLoggingImpl<
                PublicClientApplicationBuilder,
                PublicClientApplication
                >(enablePiiLogging);

        public OptionsBuilder<PublicClientApplicationBuilder> UseHttpClientFactory()
            => optionsBuilder.UseHttpClientFactoryImpl(optionsBuilder?.Name);

        public OptionsBuilder<PublicClientApplicationBuilder> UseHttpClient(
            string? name = null)
            => optionsBuilder.UseHttpClientFactoryImpl(name);
    }

    extension(OptionsBuilder<ManagedIdentityApplicationBuilder> optionsBuilder)
    {
        public OptionsBuilder<ManagedIdentityApplicationBuilder> UseLogging(
           bool enablePiiLogging = false)
            => optionsBuilder.UseLoggingImpl<
                ManagedIdentityApplicationBuilder,
                ManagedIdentityApplication
                >(enablePiiLogging);

        public OptionsBuilder<ManagedIdentityApplicationBuilder> UseHttpClientFactory()
            => optionsBuilder.UseHttpClientFactoryImpl(optionsBuilder?.Name);

        public OptionsBuilder<ManagedIdentityApplicationBuilder> UseHttpClient(
            string? name = null)
            => optionsBuilder.UseHttpClientFactoryImpl(name);

        public OptionsBuilder<ManagedIdentityApplicationBuilder> UseManagedIdentityId(
            AppConfig.ManagedIdentityId managedIdentityId)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            optionsBuilder.Services.Configure<ManagedIdentityApplicationOptions>(
                optionsBuilder.Name,
                options => options.ManagedIdentityId = managedIdentityId
                );
            return optionsBuilder;
        }
    }
}