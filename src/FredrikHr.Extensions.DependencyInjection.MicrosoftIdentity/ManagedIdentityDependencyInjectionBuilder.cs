using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class ManagedIdentityDependencyInjectionBuilder
{
    extension(IServiceCollection services)
    {
        public ManagedIdentityServicesBuilder AddManagedIdentity()
        {
            services.InheritAll<
                DownstreamApiOptions,
                AuthorizationHeaderProviderOptions
                >();
            services.InheritAll<
                MicrosoftEntraApplicationOptions,
                IdentityApplicationOptions
                >();
            services.InheritAll<
                MicrosoftIdentityApplicationOptions,
                MicrosoftEntraApplicationOptions
                >();

            return new(services);
        }
    }
}
