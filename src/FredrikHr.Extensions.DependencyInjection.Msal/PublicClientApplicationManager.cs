using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class PublicClientApplicationManager(
    IOptionsMonitor<PublicClientApplicationOptions> applicationOptionsProvider,
    IEnumerable<IConfigureOptions<PublicClientApplicationBuilder>> configureBuilder,
    IEnumerable<IPostConfigureOptions<PublicClientApplicationBuilder>> postConfigureBuilder,
    IEnumerable<IConfigureOptions<IPublicClientApplication>> configureApplication,
    IEnumerable<IPostConfigureOptions<IPublicClientApplication>> postConfigureApplication,
    IEnumerable<IValidateOptions<IPublicClientApplication>> validateApplication
    ) : OptionsManager<IPublicClientApplication>(new Factory(
        applicationOptionsProvider,
        configureBuilder,
        postConfigureBuilder,
        configureApplication,
        postConfigureApplication,
        validateApplication
        ))
{
    internal sealed class Factory(
        IOptionsMonitor<PublicClientApplicationOptions> applicationOptionsProvider,
        IEnumerable<IConfigureOptions<PublicClientApplicationBuilder>> configureBuilder,
        IEnumerable<IPostConfigureOptions<PublicClientApplicationBuilder>> postConfigureBuilder,
        IEnumerable<IConfigureOptions<IPublicClientApplication>> configureApplication,
        IEnumerable<IPostConfigureOptions<IPublicClientApplication>> postConfigureApplication,
        IEnumerable<IValidateOptions<IPublicClientApplication>> validateApplication
        ) : OptionsFactory<IPublicClientApplication>(
            configureApplication,
            postConfigureApplication,
            validateApplication
            )
    {
        private readonly PublicClientApplicationBuilderFactory _builderFactory =
            new(applicationOptionsProvider, configureBuilder, postConfigureBuilder);

        protected override IPublicClientApplication CreateInstance(string name)
        {
            var builder = _builderFactory.Create(name);
            IPublicClientApplication clientApplication = builder.Build();
            return clientApplication;
        }
    }
}