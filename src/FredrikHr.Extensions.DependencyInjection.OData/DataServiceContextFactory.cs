using Microsoft.Extensions.Options;
using Microsoft.OData.Client;

namespace FredrikHr.Extensions.DependencyInjection.OData;

public class DataServiceContextFactory<T> : OptionsFactory<T>
    where T : DataServiceContext
{
    private readonly DataServiceContextConstructorOptions? _inlineOptions;
    private readonly IOptionsMonitor<DataServiceContextConstructorOptions>? _optionsProvider;
    private readonly IEnumerable<IConfigureOptions<T>> _setups;
    private readonly IEnumerable<IPostConfigureOptions<T>> _postConfigures;
    private readonly IEnumerable<IValidateOptions<T>> _validations;

    public DataServiceContextFactory(
        IOptionsMonitor<DataServiceContextConstructorOptions> optionsProvider,
        IEnumerable<IConfigureOptions<T>> setups,
        IEnumerable<IPostConfigureOptions<T>> postConfigures,
        IEnumerable<IValidateOptions<T>> validations
    ) : base(setups, postConfigures, validations)
    {
        _optionsProvider = optionsProvider;
        _setups = setups;
        _postConfigures = postConfigures;
        _validations = validations;
    }

    private DataServiceContextFactory(
        DataServiceContextConstructorOptions constructorOptions,
        IEnumerable<IConfigureOptions<T>> setups,
        IEnumerable<IPostConfigureOptions<T>> postConfigures,
        IEnumerable<IValidateOptions<T>> validations
    ) : base(setups, postConfigures, validations)
    {
        _inlineOptions = constructorOptions;
        _setups = setups;
        _postConfigures = postConfigures;
        _validations = validations;
    }

    protected override T CreateInstance(string name)
    {
        DataServiceContextConstructorOptions constructorOptions =
            _inlineOptions ??
            _optionsProvider?.Get(name) ??
            throw new InvalidOperationException(
                $"Unable to retrieve an instance of {typeof(DataServiceContextConstructorOptions)} in order to construct an instance of {typeof(T)}."
                );
        Uri serviceRootUri = new(constructorOptions.ServiceRootUrl);
        object?[] constructorArguments = constructorOptions.MaxProtocolVersion switch
        {
            ODataProtocolVersion maxProtocolVersion =>
                [serviceRootUri, maxProtocolVersion],
            null => [serviceRootUri],
        };
        var instance = (T)(Activator.CreateInstance(typeof(T), constructorArguments) ??
            throw new InvalidOperationException(
                $"Unable to create an DataServiceContext instance of type {typeof(T)}."
                ));
        return instance;
    }
}
