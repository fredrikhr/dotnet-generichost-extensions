using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Client;

public static class MsalAcquireTokenBuilderOptions
{
    extension<T> (T builder)
        where T: BaseAbstractAcquireTokenParameterBuilder<T>
    {
        public T ApplyOptions(IServiceProvider? serviceProvider, string? name = default)
            => builder.ApplyOptions(requestMessage: null, serviceProvider, name);

        public T ApplyOptions(
            HttpRequestMessage? requestMessage,
            IServiceProvider? serviceProvider = null,
            string? name = default
            )
        {
            IEnumerable<IConfigureOptions<T>> configs = requestMessage?
                .GetOptionEnumerableByType<IConfigureOptions<T>>() ?? [];
            ApplyConfigureOptions(builder, configs, name);
            configs = serviceProvider?.GetServices<
                IConfigureOptions<T>
                >() ?? [];
            ApplyConfigureOptions(builder, configs, name);

            IEnumerable<IPostConfigureOptions<T>> postConfigs = requestMessage?
                .GetOptionEnumerableByType<IPostConfigureOptions<T>>() ?? [];
            ApplyPostConfigureOptions(builder, postConfigs, name);
            postConfigs = serviceProvider?.GetServices<
                IPostConfigureOptions<T>
                >() ?? [];
            ApplyPostConfigureOptions(builder, postConfigs, name);

            return builder;

            static void ApplyConfigureOptions(T builder, IEnumerable<IConfigureOptions<T>> configs, string? name)
            {
                if (configs is IConfigureOptions<T>[] configsArray)
                {
                    foreach (IConfigureOptions<T> config in configsArray)
                    {
                        ApplyConfigureOption(builder, config, name);
                    }
                }
                else
                {
                    foreach (IConfigureOptions<T> config in configs)
                    {
                        ApplyConfigureOption(builder, config, name);
                    }
                }
            }

            static void ApplyConfigureOption(T builder, IConfigureOptions<T> config, string? name)
            {
                switch (config)
                {
                    case IConfigureNamedOptions<T> namedConfig:
                        namedConfig.Configure(name, builder);
                        break;
                    case IConfigureOptions<T>:
                        config.Configure(builder);
                        break;
                }
            }

            static void ApplyPostConfigureOptions(T builder, IEnumerable<IPostConfigureOptions<T>> configs, string? name)
            {
                if (configs is IPostConfigureOptions<T>[] configsArray)
                {
                    foreach (IPostConfigureOptions<T> config in configsArray)
                    {
                        config.PostConfigure(name, builder);
                    }
                }
                else
                {
                    foreach (IPostConfigureOptions<T> config in configs)
                    {
                        config.PostConfigure(name, builder);
                    }
                }
            }
        }
    }
}