using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client.Extensions.Msal;

internal sealed class StorageCreationPropertiesFactory(
    IOptionsMonitor<StorageCreationPropertiesBuilder> builderProvider,
    IEnumerable<IValidateOptions<StorageCreationProperties>> validations
    ) : IOptionsFactory<StorageCreationProperties>
{
    private readonly IValidateOptions<StorageCreationProperties>[] _validations =
        [.. validations ?? []];

    public StorageCreationProperties Create(string name)
    {
        var builder = builderProvider.Get(name);
        var instance = builder.Build();
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
}
