namespace Microsoft.Extensions.Options;

public class PostConfigureAllNamedOptions<TOptions, TDep>(
    TDep dependency,
    Action<string?, TDep, TOptions> configureOptions
    ) : IPostConfigureOptions<TOptions> where TOptions : class
{
    public Action<string?, TDep, TOptions> ConfigureOptions { get; } =
        configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));

    public void PostConfigure(string? name, TOptions options)
    {
        ConfigureOptions(name, dependency, options);
    }
}
