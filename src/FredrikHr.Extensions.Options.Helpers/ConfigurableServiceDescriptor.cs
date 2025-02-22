using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Options;

public static class ConfigurableServiceDescriptor
{
    public static Func<IServiceProvider, TService> Factory<TService>(
        string? name = null)
        where TService : class
        => Factory(static sp => ActivatorUtilities.CreateInstance<TService>(sp), name);

    public static Func<IServiceProvider, TService> Factory<TService>(
        Func<IServiceProvider, TService> createInstance,
        string? name = null)
        where TService : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createInstance);
#else
        _ = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
#endif
        return CreateConfiguredImplementation;

        TService CreateConfiguredImplementation(IServiceProvider serviceProvider)
        {
            TService instance = createInstance(serviceProvider);
            foreach (var setup in serviceProvider.GetServices<IConfigureOptions<TService>>())
            {
                if (setup is IConfigureNamedOptions<TService> namedSetup)
                {
                    namedSetup.Configure(name, instance);
                }
                else if (name == Options.DefaultName)
                {
                    setup.Configure(instance);
                }
            }
            foreach (var post in serviceProvider.GetServices<IPostConfigureOptions<TService>>())
            {
                post.PostConfigure(name, instance);
            }
            var validationFailures = new List<string>();
            foreach (var validate in serviceProvider.GetServices<IValidateOptions<TService>>())
            {
                ValidateOptionsResult result = validate.Validate(name, instance);
                if (result is not null && result.Failed)
                {
                    validationFailures.AddRange(result.Failures);
                }
            }
            return validationFailures.Count == 0 ? instance
                : throw new OptionsValidationException(
                    name ?? Options.DefaultName,
                    typeof(TService),
                    validationFailures
                    );
        }
    }

    public static Func<IServiceProvider, TService> Factory<TService, TImplementation>(
        string? name = null)
        where TService : class
        where TImplementation : class, TService
        => Factory<TService, TImplementation>(static sp => ActivatorUtilities.CreateInstance<TImplementation>(sp), name);

    public static Func<IServiceProvider, TService> Factory<TService, TImplementation>(
        Func<IServiceProvider, TImplementation> createInstance,
        string? name = null)
        where TService : class
        where TImplementation : class, TService
        => Factory<TService>(createInstance, name);
}