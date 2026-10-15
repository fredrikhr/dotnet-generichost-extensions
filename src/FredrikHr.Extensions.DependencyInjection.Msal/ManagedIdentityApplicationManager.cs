using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal sealed class ManagedIdentityApplicationManager(
    IOptionsMonitor<ManagedIdentityApplicationOptions> applicationOptionsProvider,
    IEnumerable<IConfigureOptions<ManagedIdentityApplicationBuilder>> configureBuilder,
    IEnumerable<IPostConfigureOptions<ManagedIdentityApplicationBuilder>> postConfigureBuilder,
    IEnumerable<IConfigureOptions<IManagedIdentityApplication>> configureApplication,
    IEnumerable<IPostConfigureOptions<IManagedIdentityApplication>> postConfigureApplication,
    IEnumerable<IValidateOptions<IManagedIdentityApplication>> validateApplication,
    AppConfig.ManagedIdentityId? managedIdentityId = default
    ) : OptionsManager<IManagedIdentityApplication>(new Factory(
        applicationOptionsProvider,
        configureBuilder,
        postConfigureBuilder,
        configureApplication,
        postConfigureApplication,
        validateApplication,
        managedIdentityId
        ))
{
    internal sealed class Factory(
        IOptionsMonitor<ManagedIdentityApplicationOptions> applicationOptionsProvider,
        IEnumerable<IConfigureOptions<ManagedIdentityApplicationBuilder>> configureBuilder,
        IEnumerable<IPostConfigureOptions<ManagedIdentityApplicationBuilder>> postConfigureBuilder,
        IEnumerable<IConfigureOptions<IManagedIdentityApplication>> configureApplication,
        IEnumerable<IPostConfigureOptions<IManagedIdentityApplication>> postConfigureApplication,
        IEnumerable<IValidateOptions<IManagedIdentityApplication>> validateApplication,
        AppConfig.ManagedIdentityId? managedIdentityId = default
        ) : OptionsFactory<IManagedIdentityApplication>(
            configureApplication,
            postConfigureApplication,
            validateApplication
            )
    {
        private readonly ManagedIdentityApplicationBuilderFactory _builderFactory =
            new(applicationOptionsProvider, configureBuilder, postConfigureBuilder, managedIdentityId);

        protected override IManagedIdentityApplication CreateInstance(string name)
        {
            var builder = _builderFactory.Create(name);
            IManagedIdentityApplication clientApplication = builder.Build();
            return clientApplication;
        }
    }
}