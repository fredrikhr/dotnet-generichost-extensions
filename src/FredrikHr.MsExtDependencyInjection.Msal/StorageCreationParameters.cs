namespace Microsoft.Identity.Client.Extensions.Msal;

public class StorageCreationParameters
{
    public required string CacheDirectory { get; set; }
    public required string CacheName { get; set; }
}
