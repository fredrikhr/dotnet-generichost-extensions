namespace Microsoft.Extensions.Options;

public class InheritedConfigureAllOptions<TOptions, TOptionsBase>(
    IEnumerable<IConfigureOptions<TOptionsBase>> configureBaseOptions
    ) : IConfigureNamedOptions<TOptions>
    where TOptions : class, TOptionsBase
    where TOptionsBase : class
{
    public void Configure(string? name, TOptions options)
    {
        if (typeof(TOptions) == typeof(TOptionsBase)) return;
        foreach (var configureOptions in configureBaseOptions)
        {
            if (configureOptions is IConfigureNamedOptions<TOptionsBase> configureNamedOptions)
                configureNamedOptions.Configure(name, options);
            else
                configureOptions.Configure(options);
        }
    }

    public void Configure(TOptions options) => Configure(null, options);
}
