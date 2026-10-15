namespace Microsoft.Extensions.Logging.XUnit;

public sealed class XUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
        => new XUnitLogger(categoryName);

    public void Dispose() { }
}
