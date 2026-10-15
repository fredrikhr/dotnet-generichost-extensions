using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Configuration;
#pragma warning restore IDE0130 // Namespace does not match folder structure

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block")]
public static class UserSecretsPathProviderDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUserSecretsPathProvider<T>()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<
                IUserSecretsPathProvider,
                UserSecretsPathProvider<T>
                >();

            return services;
        }
    }
}