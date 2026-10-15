using System.Security.Claims;

using Microsoft.Extensions.Options;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity;

public class TokenAcquirerService(
    ITokenAcquirerFactory acquirerFactory,
    IAuthenticationSchemeInformationProvider? authSchemeInformationProvider = null)
    : ITokenAcquirer
{
    private readonly ITokenAcquirerFactory _acquirerFactory = acquirerFactory
        ?? throw new ArgumentNullException(nameof(acquirerFactory));
    private readonly IAuthenticationSchemeInformationProvider _authSchemeInformationProvider
        = authSchemeInformationProvider
        ?? new DefaultAuthenticationSchemeInformationProvider();

    private string GetAcquirerFactoryOptionName(
        AcquireTokenOptions? tokenAcquisitionOptions)
        => _authSchemeInformationProvider.GetEffectiveAuthenticationScheme(
            tokenAcquisitionOptions?.AuthenticationOptionsName
            ) ?? Options.DefaultName;


    public Task<AcquireTokenResult> GetTokenForAppAsync(
        string scope,
        AcquireTokenOptions? tokenAcquisitionOptions = null,
        CancellationToken cancellationToken = default)
    {
        ITokenAcquirer acquirer = _acquirerFactory.GetTokenAcquirer(
            GetAcquirerFactoryOptionName(tokenAcquisitionOptions)
            );
        return acquirer.GetTokenForAppAsync(
            scope,
            tokenAcquisitionOptions,
            cancellationToken);
    }

    public Task<AcquireTokenResult> GetTokenForUserAsync(
        IEnumerable<string> scopes,
        AcquireTokenOptions? tokenAcquisitionOptions = null,
        ClaimsPrincipal? user = null,
        CancellationToken cancellationToken = default)
    {
        ITokenAcquirer acquirer = _acquirerFactory.GetTokenAcquirer(
            GetAcquirerFactoryOptionName(tokenAcquisitionOptions)
            );
        return acquirer.GetTokenForUserAsync(
            scopes,
            tokenAcquisitionOptions,
            user,
            cancellationToken);
    }
}
