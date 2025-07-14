using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed class MsalCacheHelperValidateOptions(
    string? name = null
    ) : ValidateOptions<MsalCacheHelper>(
        name,
        IsValid,
        "Failed to verify MSAL Cache persistence.")
{
    private static bool IsValid(MsalCacheHelper cache)
    {
        try
        {
            cache.VerifyPersistence();
            return true;
        }
        catch (MsalCachePersistenceException)
        {
            return false;
        }
    }
}