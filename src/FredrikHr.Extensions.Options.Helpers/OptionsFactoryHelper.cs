using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Options;

public static class OptionsFactoryHelper
{
    /// <seealso cref="OptionsFactory{TOptions}.Create(string)"/>
    public static void Apply<TOptions>(
        TOptions instance,
        string? optionsName,
        IServiceProvider serviceProvider)
        where TOptions : class
        => Apply(
            instance,
            optionsName,
            serviceProvider.GetServices<IConfigureOptions<TOptions>>(),
            serviceProvider.GetServices<IPostConfigureOptions<TOptions>>(),
            serviceProvider.GetServices<IValidateOptions<TOptions>>());

    /// <seealso cref="OptionsFactory{TOptions}.Create(string)"/>
    public static void Apply<TOptions>(
        TOptions instance,
        string? optionsName,
        IEnumerable<IConfigureOptions<TOptions>>? setups = null,
        IEnumerable<IPostConfigureOptions<TOptions>>? postConfigures = null,
        IEnumerable<IValidateOptions<TOptions>>? validations = null)
        where TOptions : class
    {
        foreach (IConfigureOptions<TOptions> setup in setups ?? [])
        {
            if (setup is IConfigureNamedOptions<TOptions> namedSetup)
            {
                namedSetup.Configure(optionsName, instance);
            }
            else if (optionsName == Options.DefaultName)
            {
                setup.Configure(instance);
            }
        }
        foreach (IPostConfigureOptions<TOptions> post in postConfigures ?? [])
        {
            post.PostConfigure(optionsName, instance);
        }

        IValidateOptions<TOptions>[] validationsArray = validations switch
        {
            IValidateOptions<TOptions>[] inArray => inArray,
            null => [],
            _ => [..validations],
        };
        if (validationsArray.Length > 0)
        {
            List<string> failures = [];
            foreach (IValidateOptions<TOptions> validate in validationsArray ?? [])
            {
                ValidateOptionsResult result = validate.Validate(optionsName, instance);
                if (result is not null && result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }
            if (failures.Count > 0)
            {
                throw new OptionsValidationException(
                    optionsName ?? Options.DefaultName,
                    typeof(TOptions),
                    failures);
            }
        }
    }
}
