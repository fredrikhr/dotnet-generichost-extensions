using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Configuration;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class UserSecretsPathProviderExtensions
{
    public static IServiceCollection AddUserSecretsPathProvider<T>(
        this IServiceCollection services
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        _ = services ?? throw new ArgumentNullException(nameof(services));
#endif

        services.AddSingleton<
            IUserSecretsPathProvider,
            UserSecretsPathProvider<T>
            >();

        return services;
    }
}