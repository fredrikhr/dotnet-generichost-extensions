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
        StorageCreationProperties instance = builderProvider.Get(name).Build();
        if (_validations.Length > 0)
        {
            List<string> failures = [];
            foreach (IValidateOptions<StorageCreationProperties> validate in _validations)
            {
                ValidateOptionsResult result = validate.Validate(name, instance);
                if (result is not null && result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }
            if (failures.Count > 0)
            {
                throw new OptionsValidationException(
                    name ?? Options.DefaultName,
                    typeof(StorageCreationProperties),
                    failures
                    );
            }
        }
        return instance;
    }
}
