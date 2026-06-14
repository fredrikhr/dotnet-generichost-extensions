namespace Microsoft.Extensions.DependencyInjection.Http;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = "extension block"
    )]
public static class HttpMessageServiceProviderOptions
{
    public const string ServiceProviderOptionsKey = "Microsoft.Extensions.DependencyInjection.ServiceProvider";

    extension(HttpRequestMessage request)
    {
        public HttpRequestMessage WithServiceProvider(IServiceProvider serviceProvider)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(serviceProvider);
#else
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            _ = request ?? throw new ArgumentNullException(nameof(request));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            _ = serviceProvider
                ?? throw new ArgumentNullException(nameof(serviceProvider));
#endif

#if NET6_0_OR_GREATER
            request.Options.Set(new(ServiceProviderOptionsKey), serviceProvider);
#else
            request.Properties[ServiceProviderOptionsKey] =
                serviceProvider;
#endif

            return request;
        }

        public IServiceProvider? GetServiceProvider()
        {
#if NET6_0_OR_GREATER
            IServiceProvider? serviceProvider = null;
            request?.Options.TryGetValue(
                new(ServiceProviderOptionsKey),
                out serviceProvider
                );
            return serviceProvider;
#else
            object? serviceProviderObject = null;
            request?.Properties.TryGetValue(
                ServiceProviderOptionsKey,
                out serviceProviderObject
                );
            return serviceProviderObject as IServiceProvider;
#endif
        }
    }
}
