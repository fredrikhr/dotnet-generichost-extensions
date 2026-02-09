namespace Microsoft.Extensions.Configuration.UserSecrets;

public interface IUserSecretsPathProvider
{
    string DirectoryPath { get; }
}
