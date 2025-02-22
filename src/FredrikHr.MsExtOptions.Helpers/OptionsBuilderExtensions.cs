namespace Microsoft.Extensions.Options;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> UseInheritedConfigure<TOptions, TOptionsBase>(
        this OptionsBuilder<TOptions> optionsBuilder
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif
        if (typeof(TOptions) != typeof(TOptionsBase))
        {
            optionsBuilder.Configure<IEnumerable<IConfigureOptions<TOptionsBase>>>((opts, setups) =>
            {
                foreach (IConfigureOptions<TOptionsBase> setup in setups)
                {
                    if (setup is IConfigureNamedOptions<TOptionsBase> namedSetup)
                    {
                        namedSetup.Configure(optionsBuilder.Name, opts);
                    }
                    else if (optionsBuilder.Name == Options.DefaultName)
                    {
                        setup.Configure(opts);
                    }
                }
            });
        }
        return optionsBuilder;
    }

    public static OptionsBuilder<TOptions> UseInheritedPostConfigure<TOptions, TOptionsBase>(
        this OptionsBuilder<TOptions> optionsBuilder
        )
        where TOptions : class, TOptionsBase
        where TOptionsBase : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(optionsBuilder);
#else
        _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
#endif

        if (typeof(TOptions) != typeof(TOptionsBase))
        {
            optionsBuilder.PostConfigure<IEnumerable<IPostConfigureOptions<TOptionsBase>>>((opts, setups) =>
            {
                foreach (IPostConfigureOptions<TOptionsBase> setup in setups)
                {
                    setup.PostConfigure(optionsBuilder.Name, opts);
                }
            });
        }
        return optionsBuilder;
    }
}
