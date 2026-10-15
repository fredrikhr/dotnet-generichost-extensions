using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ConfidentialClientApplicationManager(
    IOptionsMonitor<ConfidentialClientApplicationOptions> applicationOptionsProvider,
    IEnumerable<IConfigureOptions<ConfidentialClientApplicationBuilder>> configureBuilder,
    IEnumerable<IPostConfigureOptions<ConfidentialClientApplicationBuilder>> postConfigureBuilder,
    IEnumerable<IConfigureOptions<IConfidentialClientApplication>> configureApplication,
    IEnumerable<IPostConfigureOptions<IConfidentialClientApplication>> postConfigureApplication,
    IEnumerable<IValidateOptions<IConfidentialClientApplication>> validateApplication
    ) : OptionsManager<IConfidentialClientApplication>(new Factory(
        applicationOptionsProvider,
        configureBuilder,
        postConfigureBuilder,
        configureApplication,
        postConfigureApplication,
        validateApplication
        ))
{
    internal sealed class Factory(
        IOptionsMonitor<ConfidentialClientApplicationOptions> applicationOptionsProvider,
        IEnumerable<IConfigureOptions<ConfidentialClientApplicationBuilder>> configureBuilder,
        IEnumerable<IPostConfigureOptions<ConfidentialClientApplicationBuilder>> postConfigureBuilder,
        IEnumerable<IConfigureOptions<IConfidentialClientApplication>> configureApplication,
        IEnumerable<IPostConfigureOptions<IConfidentialClientApplication>> postConfigureApplication,
        IEnumerable<IValidateOptions<IConfidentialClientApplication>> validateApplication
        ) : OptionsFactory<IConfidentialClientApplication>(
            configureApplication,
            postConfigureApplication,
            validateApplication
            )
    {
        private readonly ConfidentialClientApplicationBuilderFactory _builderFactory =
            new(applicationOptionsProvider, configureBuilder, postConfigureBuilder);

        protected override IConfidentialClientApplication CreateInstance(string name)
        {
            var builder = _builderFactory.Create(name);
            IConfidentialClientApplication clientApplication = builder.Build();
            return clientApplication;
        }
    }
}