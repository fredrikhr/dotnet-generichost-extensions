using Microsoft.Extensions.Hosting;

namespace FredrikHr.Extensions.HostingStartup;

/// <summary>
/// Represents platform specific configuration that will be applied to a <see cref="IHostBuilder"/> when building an <see cref="IHost"/>.
/// </summary>
public interface IHostingStartup
{
    /// <summary>
    /// Configure the <see cref="IHostBuilder"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Configure"/> is intended to be called before user code, allowing a user to overwrite any changes made.
    /// </remarks>
    void Configure(IHostBuilder hostBuilder);
}
