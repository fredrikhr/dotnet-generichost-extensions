using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Options;

public class InheritedPostConfigureNamedOptions<TOptions, TOptionsBase>(
    string? name,
    IEnumerable<IPostConfigureOptions<TOptionsBase>> configureBaseOptions
    ) : PostConfigureOptions<
        TOptions,
        IEnumerable<IPostConfigureOptions<TOptionsBase>>
        >(name, configureBaseOptions, (options, configureBaseOptions) =>
        {
            if (typeof(TOptions) == typeof(TOptionsBase)) return;
            foreach (var configureOptions in configureBaseOptions)
            {
                configureOptions.PostConfigure(name, options);
            }
        })
    where TOptions : class, TOptionsBase
    where TOptionsBase : class
{
    internal static InheritedPostConfigureNamedOptions<TOptions, TOptionsBase> CreateInstance(
        IServiceProvider serviceProvider,
        string? name
        )
    {
        var configureBaseOptions = serviceProvider.GetServices<
            IPostConfigureOptions<TOptionsBase>
            >();
        return new(name, configureBaseOptions);
    }
}