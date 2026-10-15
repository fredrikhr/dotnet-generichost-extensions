using Microsoft.Extensions.DependencyInjection;

namespace FredrikHr.Extensions.DependencyInjection.MicrosoftIdentity
{
    public class MicrosoftIdentityServicesBuilder(IServiceCollection services)
    {
        public IServiceCollection Services { get; } = services
            ?? throw new ArgumentNullException(nameof(services));
    }
}