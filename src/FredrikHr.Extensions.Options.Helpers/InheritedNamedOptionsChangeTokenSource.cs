using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Options;

public sealed class InheritedNamedOptionsChangeTokenSource<TOptions, TOptionsBase>(
    string? name,
    IEnumerable<IOptionsChangeTokenSource<TOptionsBase>> baseChangeTokenSources
    ) : IOptionsChangeTokenSource<TOptions>
{
    private readonly Func<IOptionsChangeTokenSource<TOptionsBase>, bool> _sourceFilter = baseSource =>
        name is null || string.Equals(name, baseSource.Name, StringComparison.Ordinal);

    public string? Name { get; } = name;

    public IChangeToken GetChangeToken()
    {
        if (baseChangeTokenSources is IOptionsChangeTokenSource<TOptionsBase>[] baseChangeTokenSourceArray)
        {
            List<IChangeToken> changeTokens = new(
                capacity: baseChangeTokenSourceArray.Length
                );
            foreach (
                IOptionsChangeTokenSource<TOptionsBase> baseSource
                in baseChangeTokenSourceArray
                )
            {
                if (_sourceFilter(baseSource))
                    changeTokens.Add(baseSource.GetChangeToken());
            }
            return new CompositeChangeToken(changeTokens);
        }
        return new CompositeChangeToken([
            ..baseChangeTokenSources
            .Where(_sourceFilter)
            .Select(s => s.GetChangeToken())
            ]);
    }

    internal static InheritedNamedOptionsChangeTokenSource<TOptions, TOptionsBase> CreateInstance(
        IServiceProvider serviceProvider,
        string? name
        )
    {
        var baseChangeTokenSources = serviceProvider.GetServices<
            IOptionsChangeTokenSource<TOptionsBase>
            >();
        return new(name, baseChangeTokenSources);
    }
}