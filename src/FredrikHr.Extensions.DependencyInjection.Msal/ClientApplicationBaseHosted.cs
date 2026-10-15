namespace Microsoft.Identity.Client;

internal class ClientApplicationBaseHosted<TApplication>(
    string? name,
    IServiceProvider serviceProvider,
    TApplication clientApplication
    ) : ApplicationBaseHosted<TApplication>(
        name,
        serviceProvider,
        clientApplication
        ), IClientApplicationBase
    where TApplication : IClientApplicationBase
{
    public IAppConfig AppConfig => ClientApplication.AppConfig;

    public ITokenCache UserTokenCache => ClientApplication.UserTokenCache;

    public string Authority => ClientApplication.Authority;

    public AcquireTokenSilentParameterBuilder AcquireTokenSilent(
        IEnumerable<string> scopes,
        IAccount account
        ) => ClientApplication.AcquireTokenSilent(scopes, account)
            .UseConfiguredOptions(ServiceProvider, Name);

    public AcquireTokenSilentParameterBuilder AcquireTokenSilent(
        IEnumerable<string> scopes,
        string loginHint
        ) => ClientApplication.AcquireTokenSilent(scopes, loginHint)
            .UseConfiguredOptions(ServiceProvider, Name);

    public Task<IAccount> GetAccountAsync(string identifier)
        => ClientApplication.GetAccountAsync(identifier);

    public Task<IEnumerable<IAccount>> GetAccountsAsync()
        => ClientApplication.GetAccountsAsync();

    public Task<IEnumerable<IAccount>> GetAccountsAsync(string userFlow)
        => ClientApplication.GetAccountsAsync(userFlow);

    public Task RemoveAsync(IAccount account)
        => ClientApplication.RemoveAsync(account);
}
