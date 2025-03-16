using Microsoft.Extensions.Options;

namespace Microsoft.SharePoint.Client;

public sealed class ClientContextFactory : OptionsFactory<ClientContext>
{
    private readonly ClientContextConstructorOptions? _inlineOptions;
    private readonly IOptionsMonitor<ClientContextConstructorOptions>? _optionsProvider;
    private readonly IEnumerable<IConfigureOptions<ClientContext>> _setups;
    private readonly IEnumerable<IPostConfigureOptions<ClientContext>> _postConfigures;
    private readonly IEnumerable<IValidateOptions<ClientContext>> _validations;

    public ClientContextFactory(
        IOptionsMonitor<ClientContextConstructorOptions> optionsProvider,
        IEnumerable<IConfigureOptions<ClientContext>> setups,
        IEnumerable<IPostConfigureOptions<ClientContext>> postConfigures,
        IEnumerable<IValidateOptions<ClientContext>> validations
    ) : base(setups, postConfigures, validations)
    {
        _optionsProvider = optionsProvider;
        _setups = setups;
        _postConfigures = postConfigures;
        _validations = validations;
    }

    private ClientContextFactory(
        ClientContextConstructorOptions webUrlOptions,
        IEnumerable<IConfigureOptions<ClientContext>> setups,
        IEnumerable<IPostConfigureOptions<ClientContext>> postConfigures,
        IEnumerable<IValidateOptions<ClientContext>> validations
    ) : base(setups, postConfigures, validations)
    {
        _inlineOptions = webUrlOptions;
        _setups = setups;
        _postConfigures = postConfigures;
        _validations = validations;
    }

    protected override ClientContext CreateInstance(string? name)
    {
        name ??= Options.DefaultName;
        ClientContextConstructorOptions ctorOptions = _inlineOptions
            ?? _optionsProvider!.Get(name);
        return new(ctorOptions.WebUrl);
    }

    public ClientContext Create() => Create(Options.DefaultName);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1054: URI-like parameters should not be strings",
        Justification = nameof(ClientContext)
        )]
    public ClientContext CreateWithWebUrl(string webUrl, string? name = default)
    {
        ClientContextFactory inlineFactory = new(
            new ClientContextConstructorOptions { WebUrl = webUrl },
            _setups,
            _postConfigures,
            _validations
            );
        return inlineFactory.Create(name ?? Options.DefaultName);
    }
}
