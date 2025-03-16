namespace Microsoft.SharePoint.Client;

public class ClientContextConstructorOptions()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1056: URI-like properties should not be strings",
        Justification = nameof(Extensions.Options)
        )]
    public required string WebUrl { get; set; }
}