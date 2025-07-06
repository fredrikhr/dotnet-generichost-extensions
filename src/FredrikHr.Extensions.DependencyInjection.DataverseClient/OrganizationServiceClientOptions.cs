using System.Reflection;

using Microsoft.Extensions.Options;

namespace FredrikHr.Extensions.DependencyInjection.DataverseClient;

public class OrganizationServiceClientOptions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1056: URI-like properties should not be strings",
        Justification = nameof(IOptions<OrganizationServiceClientOptions>)
        )]
    public required string ServiceUrl { get; set; }
    public TimeSpan? Timeout { get; set; }
    public bool UseStrongTypes { get; set; } = true;
    public Assembly? StrongTypesAssembly { get; set; }
    public string? HttpClientHandlerName { get; set; }
}
