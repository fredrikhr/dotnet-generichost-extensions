namespace Microsoft.Extensions.Options;

public class ConfigureAllNamedOptions<TOptions, TDep>(
    TDep dependency,
    Action<string?, TDep, TOptions> configureOptions
    ) : IConfigureNamedOptions<TOptions> where TOptions : class
{
    public Action<string?, TDep, TOptions> ConfigureOptions { get; } =
        configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));

    /// <inhertdoc cref="IConfigureNamedOptions{TOptions}.Configure(string?, TOptions)"/>
    public void Configure(string? name, TOptions options)
    {
        ConfigureOptions(name, dependency, options);
    }

    /// <inhertdoc cref="IConfigureOptions{TOptions}.Configure(TOptions)"/>
    public void Configure(TOptions options)
        => Configure(Options.DefaultName, options);
}
