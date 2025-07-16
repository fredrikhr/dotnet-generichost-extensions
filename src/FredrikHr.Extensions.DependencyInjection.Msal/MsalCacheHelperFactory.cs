using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

public sealed class MsalCacheHelperFactory :
    IOptionsFactory<Task<MsalCacheHelper>>,
    IDisposable
{
    private readonly IOptionsMonitor<StorageCreationPropertiesBuilder> _storageCreationPropsOptions;
    private readonly ILogger<MsalCacheHelper> _logger;
    private readonly IEnumerable<IConfigureOptions<MsalCacheHelper>> _setups;
    private readonly IEnumerable<IPostConfigureOptions<MsalCacheHelper>> _postConfigures;
    private readonly IValidateOptions<MsalCacheHelper>[] _validations;
    private readonly MsalCacheLoggerTraceListener _traceListener;
    private readonly IDisposable? _loggerOptionsChangeRegistration;

    internal System.Diagnostics.TraceSource TraceSource { get; } =
        new(typeof(MsalCacheHelper).FullName!);

    public MsalCacheHelperFactory(
        IOptionsMonitor<StorageCreationPropertiesBuilder> storageCreationPropsOptions,
        ILogger<MsalCacheHelper> logger,
        IEnumerable<IConfigureOptions<MsalCacheHelper>> setups,
        IEnumerable<IPostConfigureOptions<MsalCacheHelper>> postConfigures,
        IEnumerable<IValidateOptions<MsalCacheHelper>> validations,
        IOptionsMonitor<LoggerFilterOptions> loggerOptionsMonitor
        )
    {
        _storageCreationPropsOptions = storageCreationPropsOptions;
        _logger = logger;
        _setups = setups;
        _postConfigures = postConfigures;
        _validations = [.. validations ?? []];
        _traceListener = new MsalCacheLoggerTraceListener(logger);
        TraceSource.Listeners.Add(_traceListener);
        _loggerOptionsChangeRegistration = loggerOptionsMonitor?.OnChange(
            OnLoggerOptionsChanged
            );
        OnLoggerOptionsChanged(null);
    }

    Task<MsalCacheHelper> IOptionsFactory<Task<MsalCacheHelper>>.Create(string name)
    {
        return CreateAsync(name);
    }

    public async Task<MsalCacheHelper> CreateAsync(
        string? name = default
        )
    {
        StorageCreationProperties props = _storageCreationPropsOptions.Get(name)
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
            List<string> failures = [];
            foreach (IValidateOptions<MsalCacheHelper> validate in _validations)
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

    private void OnLoggerOptionsChanged(LoggerFilterOptions? options)
    {
        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.All;
        else if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.Verbose;
        else if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.Information;
        else if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.Warning;
        else if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
        else if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Critical))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
        else if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.None))
            TraceSource.Switch.Level = System.Diagnostics.SourceLevels.Off;
    }

    public void Dispose()
    {
        _loggerOptionsChangeRegistration?.Dispose();
        TraceSource.Listeners.Clear();
        _traceListener.Dispose();
    }
}