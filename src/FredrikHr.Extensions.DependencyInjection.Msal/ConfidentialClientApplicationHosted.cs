namespace Microsoft.Identity.Client;

internal class ConfidentialClientApplicationHosted(
    string? name,
    IServiceProvider serviceProvider,
    IConfidentialClientApplication clientApplication
    ) : ClientApplicationBaseHosted<IConfidentialClientApplication>(
        name,
        serviceProvider,
        clientApplication
        ), IConfidentialClientApplication
{
    public ITokenCache AppTokenCache => ClientApplication.AppTokenCache;

    public AcquireTokenByAuthorizationCodeParameterBuilder AcquireTokenByAuthorizationCode(
        IEnumerable<string> scopes,
        string authorizationCode
        ) => ClientApplication
        .AcquireTokenByAuthorizationCode(scopes, authorizationCode)
        .UseConfiguredOptions(ServiceProvider, Name)
        ;

    public AcquireTokenForClientParameterBuilder AcquireTokenForClient(
        IEnumerable<string> scopes
        ) => ClientApplication.AcquireTokenForClient(scopes)
        .UseConfiguredOptions(ServiceProvider, Name);

    public AcquireTokenOnBehalfOfParameterBuilder AcquireTokenOnBehalfOf(
        IEnumerable<string> scopes,
        UserAssertion userAssertion
        ) => ClientApplication.AcquireTokenOnBehalfOf(scopes, userAssertion)
        .UseConfiguredOptions(ServiceProvider, Name);

    public GetAuthorizationRequestUrlParameterBuilder GetAuthorizationRequestUrl(
        IEnumerable<string> scopes
        ) => ClientApplication.GetAuthorizationRequestUrl(scopes)
        .UseConfiguredOptions(ServiceProvider, Name);
}
