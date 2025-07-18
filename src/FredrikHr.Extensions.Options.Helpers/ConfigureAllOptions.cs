namespace Microsoft.Extensions.Options;

public class ConfigureAllOptions<TOptions>(
    Action<string?, TOptions> configureOptions
    ) : IConfigureNamedOptions<TOptions> where TOptions : class
{
    public Action<string?, TOptions> ConfigureOptions { get; } =
        configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));

    /// <inhertdoc cref="IConfigureNamedOptions{TOptions}.Configure(string?, TOptions)"/>
    public void Configure(string? name, TOptions options)
    {
        ConfigureOptions(name, options);
    }

    /// <inhertdoc cref="IConfigureOptions{TOptions}.Configure(TOptions)"/>
    public void Configure(TOptions options)
        => Configure(null, options);
}

public class ConfigureAllOptions<TOptions, TDep>(
    TDep dependency,
    Action<string?, TOptions, TDep> configureOptions
    ) : IConfigureNamedOptions<TOptions> where TOptions : class
{
    public Action<string?, TOptions, TDep> ConfigureOptions { get; } =
        configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));

    /// <inhertdoc cref="IConfigureNamedOptions{TOptions}.Configure(string?, TOptions)"/>
    public void Configure(string? name, TOptions options)
    {
        ConfigureOptions(name, options, dependency);
    }

    /// <inhertdoc cref="IConfigureOptions{TOptions}.Configure(TOptions)"/>
    public void Configure(TOptions options)
        => Configure(null, options);
}
