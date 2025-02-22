using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

public sealed class MsalCacheHelperFactory : IDisposable
{
    private readonly IOptionsMonitor<StorageCreationPropertiesBuilder> _storageCreationPropsOptions;
    private readonly System.Diagnostics.TraceSource _traceSource =
        new(typeof(MsalCacheHelper).FullName!);
    private readonly MsalCacheLoggerTraceListener _traceListener;

    public MsalCacheHelperFactory(
        IOptionsMonitor<StorageCreationPropertiesBuilder> storageCreationPropsOptions,
        ILogger<MsalCacheHelper> logger
        )
    {
        _storageCreationPropsOptions = storageCreationPropsOptions;
        _traceListener = new MsalCacheLoggerTraceListener(logger);
        _traceSource.Listeners.Add(_traceListener);
    }

    public Task<MsalCacheHelper> CreateMsalCacheHelperAsync(
        string? name = default
        )
    {
        var props = _storageCreationPropsOptions.Get(name)
            .Build();
        return MsalCacheHelper.CreateAsync(props, _traceSource);
    }

    public void Dispose()
    {
        _traceSource.Listeners.Clear();
        _traceListener.Dispose();
    }
}
