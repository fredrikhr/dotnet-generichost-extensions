namespace Microsoft.Extensions.Options;

public class PostConfigureAllOptions<TOptions>(
    Action<string?, TOptions> configureOptions
    ) : IPostConfigureOptions<TOptions> where TOptions : class
{
    public Action<string?, TOptions> ConfigureOptions { get; } =
        configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));

    public void PostConfigure(string? name, TOptions options)
    {
        ConfigureOptions(name, options);
    }
}

public class PostConfigureAllOptions<TOptions, TDep>(
    TDep dependency,
    Action<string?, TOptions, TDep> configureOptions
    ) : IPostConfigureOptions<TOptions> where TOptions : class
{
    public Action<string?, TOptions, TDep> ConfigureOptions { get; } =
        configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));

    public void PostConfigure(string? name, TOptions options)
    {
        ConfigureOptions(name, options, dependency);
    }
}
