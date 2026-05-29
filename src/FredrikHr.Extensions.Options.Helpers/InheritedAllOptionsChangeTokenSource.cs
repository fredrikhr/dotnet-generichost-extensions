using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Options;

public sealed class InheritedAllOptionsChangeTokenSource<TOptions, TOptionsBase>(
    IEnumerable<IOptionsChangeTokenSource<TOptionsBase>> baseChangeTokenSources
    ) : IOptionsChangeTokenSource<TOptions>
{
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
                changeTokens.Add(baseSource.GetChangeToken());
            }
            return new CompositeChangeToken(changeTokens);
        }
        return new CompositeChangeToken([
            ..baseChangeTokenSources.Select(s => s.GetChangeToken())
            ]);
    }

    string? IOptionsChangeTokenSource<TOptions>.Name => null;
}
