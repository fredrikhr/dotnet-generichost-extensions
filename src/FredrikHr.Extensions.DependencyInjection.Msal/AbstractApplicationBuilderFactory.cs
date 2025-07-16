using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

internal abstract class AbstractApplicationBuilderFactory<TBuilder, TOptions>(
    IOptionsMonitor<TOptions> optionsProvider,
    IEnumerable<IConfigureOptions<TBuilder>> setups,
    IEnumerable<IPostConfigureOptions<TBuilder>> postConfigures
    ) : OptionsFactory<TBuilder>(setups, postConfigures)
    where TBuilder : AbstractApplicationBuilder<TBuilder>
    
{
    protected abstract TBuilder CreateInstance(TOptions options);

    protected sealed override TBuilder CreateInstance(string name)
    {
        TOptions? options = optionsProvider.Get(name);
        return CreateInstance(options);
    }
}
