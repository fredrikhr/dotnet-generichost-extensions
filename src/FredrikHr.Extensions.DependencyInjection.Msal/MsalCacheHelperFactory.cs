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
    private readonly IValidateOptions<MsalCacheHelper>[] _validations;
    private readonly MsalCacheLoggerTraceListener _traceListener;

    internal System.Diagnostics.TraceSource TraceSource { get; } =
        new(typeof(MsalCacheHelper).FullName!);

    public MsalCacheHelperFactory(
        IOptionsMonitor<StorageCreationPropertiesBuilder> storageCreationPropsOptions,
        ILogger<MsalCacheHelper> logger,
        IEnumerable<IConfigureOptions<MsalCacheHelper>> setups,
        IEnumerable<IPostConfigureOptions<MsalCacheHelper>> postConfigures,
        IEnumerable<IValidateOptions<MsalCacheHelper>> validations
        )
    {
        _storageCreationPropsOptions = storageCreationPropsOptions;
        _setups = setups;
        _postConfigures = postConfigures;
        _validations = [.. validations ?? []];
        _traceListener = new MsalCacheLoggerTraceListener(logger);
        TraceSource.Listeners.Add(_traceListener);
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
        var instance = await MsalCacheHelper.CreateAsync(props, TraceSource)
            .ConfigureAwait(continueOnCapturedContext: false);
        foreach (IConfigureOptions<MsalCacheHelper> setup in _setups ?? [])
        {
            if (setup is IConfigureNamedOptions<MsalCacheHelper> namedSetup)
            {
                namedSetup.Configure(name, instance);
            }
            else if (name == Options.DefaultName || name is null)
            {
                setup.Configure(instance);
            }
        }
        foreach (IPostConfigureOptions<MsalCacheHelper> post in _postConfigures ?? [])
        {
            post.PostConfigure(name, instance);
        }
        if (_validations.Length > 0)
        {
            var failures = new List<string>();
            foreach (var validate in _validations)
            {
                ValidateOptionsResult result = validate.Validate(name, instance);
                if (result is not null && result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }
            if (failures.Count > 0)
            {
                throw new OptionsValidationException(name ?? Options.DefaultName, instance.GetType(), failures);
            }
        }

        return instance;
    }

    public void Dispose()
    {
        TraceSource.Listeners.Clear();
        _traceListener.Dispose();
    }
}
