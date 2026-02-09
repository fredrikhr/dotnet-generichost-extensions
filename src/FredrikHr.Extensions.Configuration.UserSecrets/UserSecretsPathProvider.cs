using System.Reflection;

namespace Microsoft.Extensions.Configuration.UserSecrets;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = nameof(DependencyInjection)
    )]
internal sealed class UserSecretsPathProvider<T> : IUserSecretsPathProvider
{
    private static readonly string? UserSecretsId = typeof(T).Assembly
        .GetCustomAttribute<UserSecretsIdAttribute>() switch
        {
            { UserSecretsId: string attrValue } => attrValue,
            _ => null,
        };

    private static string GetDirectoryPath()
    {
        string userSecretsFilePath = PathHelper.GetSecretsPathFromSecretsId(
            UserSecretsId!
            );
        return Path.GetDirectoryName(userSecretsFilePath)!;
    }

    public string DirectoryPath { get; } = GetDirectoryPath();
}