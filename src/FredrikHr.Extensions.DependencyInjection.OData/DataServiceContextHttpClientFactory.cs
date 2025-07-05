namespace FredrikHr.Extensions.DependencyInjection.OData;

internal sealed class DataServiceContextHttpClientFactory(
    string? name,
    IHttpClientFactory httpFactory
    ) : IHttpClientFactory
{
    private readonly string? _overrideName = name;

    public HttpClient CreateClient(string name) =>
        httpFactory.CreateClient(_overrideName ?? name);
}