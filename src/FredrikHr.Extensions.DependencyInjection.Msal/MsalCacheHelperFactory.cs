using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

public sealed class MsalCacheHelperFactory :
    IOptionsFactory<Task<MsalCacheHelper>>,
    IDisposable
{
    private readonly IOptionsMonitor<StorageCreationPropertiesBuilder> _storageCreationPropsOptions;
    private readonly IEnumerable<IConfigureOptions<MsalCacheHelper>> _setups;
    private readonly IEnumerable<IPostConfigureOptions<MsalCacheHelper>> _postConfigures;
    private readonly System.Diagnostics.TraceSource _traceSource =
        new(typeof(MsalCacheHelper).FullName!);
    private readonly MsalCacheLoggerTraceListener _traceListener;

    public MsalCacheHelperFactory(
        IOptionsMonitor<StorageCreationPropertiesBuilder> storageCreationPropsOptions,
        ILogger<MsalCacheHelper> logger,
        IEnumerable<IConfigureOptions<MsalCacheHelper>> setups,
        IEnumerable<IPostConfigureOptions<MsalCacheHelper>> postConfigures
        )
    {
        _storageCreationPropsOptions = storageCreationPropsOptions;
        _setups = setups;
        _postConfigures = postConfigures;
        _traceListener = new MsalCacheLoggerTraceListener(logger);
        _traceSource.Listeners.Add(_traceListener);
    }

    Task<MsalCacheHelper> IOptionsFactory<Task<MsalCacheHelper>>.Create(string name)
    {
        return CreateAsync(name);
    }

    public async Task<MsalCacheHelper> CreateAsync(
        string? name = default
        )
    {
        var props = _storageCreationPropsOptions.Get(name)
            .Build();
        var instance = await MsalCacheHelper.CreateAsync(props, _traceSource)
            .ConfigureAwait(continueOnCapturedContext: false);
        foreach (IConfigureOptions<MsalCacheHelper> setup in _setups)
        {
            if (setup is IConfigureNamedOptions<MsalCacheHelper> namedSetup)
            {
                namedSetup.Configure(name, instance);
            }
            else if (name == Options.DefaultName)
            {
                setup.Configure(instance);
            }
        }
        foreach (IPostConfigureOptions<MsalCacheHelper> post in _postConfigures)
        {
            post.PostConfigure(name, instance);
        }

        return instance;
    }

    public void Dispose()
    {
        _traceSource.Listeners.Clear();
        _traceListener.Dispose();
    }
}
