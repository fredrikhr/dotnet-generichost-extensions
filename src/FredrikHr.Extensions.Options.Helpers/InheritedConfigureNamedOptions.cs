using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Options;

public class InheritedConfigureNamedOptions<TOptions, TOptionsBase>(
    string? name,
    IEnumerable<IConfigureOptions<TOptionsBase>> configureBaseOptions
    ) : ConfigureNamedOptions<
        TOptions,
        IEnumerable<IConfigureOptions<TOptionsBase>>
        >(name, configureBaseOptions, (options, configureBaseOptions) =>
    {
        if (typeof(TOptions) == typeof(TOptionsBase)) return;
        foreach (IConfigureOptions<TOptionsBase> configureOptions in configureBaseOptions)
        {
            if (configureOptions is IConfigureNamedOptions<TOptionsBase> configureNamedOptions)
                configureNamedOptions.Configure(name, options);
            else
                configureOptions.Configure(options);
        }
    }) where TOptions : class, TOptionsBase where TOptionsBase : class
{
    internal static InheritedConfigureNamedOptions<TOptions, TOptionsBase> CreateInstance(
        IServiceProvider serviceProvider,
        string? name
        )
    {
        var configureBaseOptions = serviceProvider.GetServices<
            IConfigureOptions<TOptionsBase>
            >();
        return new(name, configureBaseOptions);
    }
}
