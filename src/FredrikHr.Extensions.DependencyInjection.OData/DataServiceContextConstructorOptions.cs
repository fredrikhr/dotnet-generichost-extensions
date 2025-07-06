using Microsoft.Extensions.Options;
using Microsoft.OData.Client;

namespace FredrikHr.Extensions.DependencyInjection.OData;

public class DataServiceContextConstructorOptions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1056: URI-like properties should not be strings",
        Justification = nameof(IOptions<DataServiceContextConstructorOptions>)
        )]
    public required string ServiceRootUrl { get; set; }

    public ODataProtocolVersion? MaxProtocolVersion { get; set; }
}