namespace Microsoft.Identity.Client;

internal class ApplicationBaseHosted<TApplication>(
    string? name,
    IServiceProvider serviceProvider,
    TApplication clientApplication
    ) : IApplicationBase
    where TApplication : IApplicationBase
{
    protected internal string? Name { get; } = name;
    protected IServiceProvider ServiceProvider { get; } = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));
    protected TApplication ClientApplication { get; } = clientApplication
        ?? throw new ArgumentNullException(nameof(clientApplication));
}
