using System.Diagnostics;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.TUnit;

namespace FredrikHr.Hosting.Msal.TUnit;

public class TUnitHostBuilderFactory
{
    private static readonly EmbeddedFileProvider AssemblyEmbeddedResourceFiles =
        new(typeof(TUnitHostBuilderFactory).Assembly);

    public static readonly string UserSecretsDirectoryPath = Path
        .GetDirectoryName(
            PathHelper.GetSecretsPathFromSecretsId(
                typeof(TUnitHostBuilderFactory).Assembly!
                .GetCustomAttribute<UserSecretsIdAttribute>()
                ?.UserSecretsId ?? Guid.Empty.ToString()
            )
        )!;

    private static void ConfigureHostConfiguration(IConfigurationBuilder config)
        => ConfigureHostConfiguration(config, out int _);

    private static void ConfigureHostConfiguration(IConfigurationBuilder config, out int insertIndex)
    {
        for (insertIndex = 0; insertIndex < config.Sources.Count; insertIndex++)
        {
            if (config.Sources[insertIndex] is not MemoryConfigurationSource)
                break;
        }
        config.Insert(insertIndex, static config => config.AddJsonFile(
            AssemblyEmbeddedResourceFiles,
            path: "appsettings.json",
            optional: true,
            reloadOnChange: false
            ));
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
        => ConfigureAppConfiguration(context, config, 0);

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config, int insertIndex)
    {
        if (context.HostingEnvironment?.EnvironmentName is not { Length: > 0 } env)
            return;
        for (; insertIndex < config.Sources.Count; insertIndex++)
        {
            if (config.Sources[insertIndex] is MemoryConfigurationSource)
                break;
        }
        for (; insertIndex < config.Sources.Count; insertIndex++)
        {
            if (config.Sources[insertIndex] is not MemoryConfigurationSource)
                break;
        }
        config.Insert(insertIndex, c => c.AddJsonFile(
            AssemblyEmbeddedResourceFiles,
            path: $"appsettings.{env}.json",
            optional: true,
            reloadOnChange: false
            ));
    }

    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        ConfigureDebugLogging(logging);
        logging.AddTUnit();

        [Conditional("DEBUG")]
        static void ConfigureDebugLogging(ILoggingBuilder logging)
        {
            logging.AddDebug();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.UseTUnitTestContextLifetime();
    }

    public IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();
        hostBuilder.ConfigureHostConfiguration(ConfigureHostConfiguration);
        hostBuilder.ConfigureAppConfiguration(ConfigureAppConfiguration);
        hostBuilder.ConfigureLogging(ConfigureLogging);
        hostBuilder.ConfigureServices(ConfigureServices);
        return hostBuilder;
    }

    public HostApplicationBuilder CreateHostAppBuilder()
    {
        HostApplicationBuilderSettings hostSettings = new()
        { EnvironmentName = Environments.Development };
        var hostBuilder = Host.CreateApplicationBuilder(hostSettings);
        HostBuilderContext context = new(new Dictionary<object, object>())
        {
            HostingEnvironment = hostBuilder.Environment,
            Configuration = hostBuilder.Configuration,
        };
        ConfigureHostConfiguration(hostBuilder.Configuration, out int configInsertIndex);
        ConfigureAppConfiguration(context, hostBuilder.Configuration, configInsertIndex);
        ConfigureLogging(hostBuilder.Logging);
        ConfigureServices(hostBuilder.Services);
        return hostBuilder;
    }
}
