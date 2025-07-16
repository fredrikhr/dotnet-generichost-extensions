namespace Microsoft.Extensions.Options;

public class InheritedPostConfigureAllOptions<TOptions, TOptionsBase>(
    IEnumerable<IPostConfigureOptions<TOptionsBase>> configureBaseOptions
    ) : IPostConfigureOptions<TOptions>
    where TOptions : class, TOptionsBase
    where TOptionsBase : class
{
    public void PostConfigure(string? name, TOptions options)
    {
        if (typeof(TOptions) == typeof(TOptionsBase)) return;
        foreach (IPostConfigureOptions<TOptionsBase> configureOptions in configureBaseOptions)
        {
            configureOptions.PostConfigure(name, options);
        }
    }
}
