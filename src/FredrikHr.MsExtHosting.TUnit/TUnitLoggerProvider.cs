namespace Microsoft.Extensions.Logging.TUnit;

public sealed class TUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TUnitLogger(categoryName);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1063: Implement IDisposable Correctly",
        Justification = "noop"
        )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0079: Remove unnecessary suppression"
        )]
    void IDisposable.Dispose()
    {
    }
}
