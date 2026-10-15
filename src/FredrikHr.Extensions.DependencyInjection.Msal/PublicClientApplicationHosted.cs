using System.ComponentModel;
using System.Security;

namespace Microsoft.Identity.Client;

internal class PublicClientApplicationHosted(
    string? name,
    IServiceProvider serviceProvider,
    IPublicClientApplication clientApplication)
    : ClientApplicationBaseHosted<IPublicClientApplication>(
        name,
        serviceProvider,
        clientApplication),
    IPublicClientApplication
{
    /// <inheritdoc cref="IPublicClientApplication.IsSystemWebViewAvailable" />
    public bool IsSystemWebViewAvailable
        => ClientApplication.IsSystemWebViewAvailable;

    /// <inheritdoc cref="IPublicClientApplication.AcquireTokenInteractive" />
    public AcquireTokenInteractiveParameterBuilder AcquireTokenInteractive(
        IEnumerable<string> scopes)
        => ClientApplication.AcquireTokenInteractive(scopes)
        .UseConfiguredOptions(ServiceProvider, Name);

    /// <inheritdoc cref="IPublicClientApplication.AcquireTokenWithDeviceCode" />
    public AcquireTokenWithDeviceCodeParameterBuilder AcquireTokenWithDeviceCode(
        IEnumerable<string> scopes,
        Func<DeviceCodeResult, Task> deviceCodeResultCallback)
        => ClientApplication
        .AcquireTokenWithDeviceCode(scopes, deviceCodeResultCallback)
        .UseConfiguredOptions(ServiceProvider, Name);

    /// <inheritdoc cref="IPublicClientApplication.AcquireTokenByIntegratedWindowsAuth" />
    [Obsolete("This API is no longer recommended. For scenarios requiring SSO with the Windows OS's default account, please transition to using WAM.", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AcquireTokenByIntegratedWindowsAuthParameterBuilder AcquireTokenByIntegratedWindowsAuth(
        IEnumerable<string> scopes)
        => ClientApplication.AcquireTokenByIntegratedWindowsAuth(scopes)
        .UseConfiguredOptions(ServiceProvider, Name);

    /// <inheritdoc cref="IPublicClientApplication.AcquireTokenByUsernamePassword(IEnumerable{string}, string, SecureString)" />
    [Obsolete("This API has been deprecated, use a more secure flow. See https://aka.ms/msal-ropc-migration for migration guidance", false)]
    [EditorBrowsable(EditorBrowsableState.Never)] // deprecated, this API is no longer supported
    public AcquireTokenByUsernamePasswordParameterBuilder AcquireTokenByUsernamePassword(
        IEnumerable<string> scopes,
        string username,
        SecureString password)
        => ClientApplication
        .AcquireTokenByUsernamePassword(scopes, username, password)
        .UseConfiguredOptions(ServiceProvider, Name);

    /// <inheritdoc cref="IPublicClientApplication.AcquireTokenByUsernamePassword(IEnumerable{string}, string, string)" />
    [Obsolete("This API has been deprecated, use a more secure flow. See https://aka.ms/msal-ropc-migration for migration guidance", false)]
    [EditorBrowsable(EditorBrowsableState.Never)] // deprecated, this API is no longer supported
    public AcquireTokenByUsernamePasswordParameterBuilder AcquireTokenByUsernamePassword(
        IEnumerable<string> scopes,
        string username,
        string password)
        => ClientApplication
        .AcquireTokenByUsernamePassword(scopes, username, password)
        .UseConfiguredOptions(ServiceProvider, Name);
}
