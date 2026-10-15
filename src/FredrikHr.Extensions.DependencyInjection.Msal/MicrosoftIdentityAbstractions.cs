using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;

namespace FredrikHr.Extensions.DependencyInjection.Msal;

internal class Foo<TMsalOptions, TIdentityOptions>(
    IOptionsMonitor<TIdentityOptions> identityOptionsProvider)
    : IConfigureNamedOptions<TMsalOptions>
    where TMsalOptions : ApplicationOptions
    where TIdentityOptions : IdentityApplicationOptions
{
    public void Configure(string? name, TMsalOptions options)
    {
        TIdentityOptions identityOptions = identityOptionsProvider.Get(name);
        options.ClientId = identityOptions.ClientId;
        
    }

    void IConfigureOptions<TMsalOptions>.Configure(
        TMsalOptions options)
        => Configure(default, options);
}
