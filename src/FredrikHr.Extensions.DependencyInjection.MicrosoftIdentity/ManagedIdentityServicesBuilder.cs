using Microsoft.Extensions.DependencyInjection;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity
{
    public class ManagedIdentityServicesBuilder(IServiceCollection services)
    {
        public IServiceCollection Services { get; } = services
            ?? throw new ArgumentNullException(nameof(services));
    }
}