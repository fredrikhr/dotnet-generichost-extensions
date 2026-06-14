using Microsoft.Extensions.Options;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity;

internal sealed class DefaultAuthenticationSchemeInformationProvider
    : IAuthenticationSchemeInformationProvider
{
    public string GetEffectiveAuthenticationScheme(string? authenticationScheme)
        => authenticationScheme ?? Options.DefaultName;
}