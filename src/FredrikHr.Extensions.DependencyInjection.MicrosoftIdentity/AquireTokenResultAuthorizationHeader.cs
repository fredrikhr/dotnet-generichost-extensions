namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity;

internal static class AquireTokenResultAuthorizationHeader
{
    public const string DefaultTokenType = "Bearer";

    extension(AcquireTokenResult result)
    {
        public string GetAuthorizationHeaderValue()
            => $"{result.TokenType ?? DefaultTokenType} {result.AccessToken}";
    }
}