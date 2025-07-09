namespace Microsoft.Extensions.Options;

public class PostConfigureAllNamedOptions<TOptions, TDep>(
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
