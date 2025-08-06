using System.Reflection;
using System.Runtime.Loader;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FredrikHr.Extensions.HostingStartup;

public static class HostingStartupHostBuilderExtensions
{
    public static HostApplicationBuilder UseHostingStartupAssemblyLoading(
        this HostApplicationBuilder hostBuilder
        )
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        IHostBuilder hostBuilderWrapper = HostApplicationBuilderUnsafeAccessor
            .AsHostBuilder(hostBuilder);
        UseHostingStartupAssemblyLoading(hostBuilderWrapper);
        return hostBuilder;
    }

    public static IHostBuilder UseHostingStartupAssemblyLoading(
        this IHostBuilder hostBuilder
        )
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        return hostBuilder.ConfigureHostConfiguration(config =>
        {
            ConfigurationBuilder dynamicConfigBuilder = new();
            foreach (var configSource in config.Sources)
            {
                dynamicConfigBuilder.Add(configSource);
            }
            IConfigurationRoot dynamicConfig = dynamicConfigBuilder.Build();
            try
            {
                if (ParseBooleanLikeValue(dynamicConfig[HostingStartupDefaults.PreventHostingStartupKey]) ?? false)
                {
                    return;
                }

                AssemblyLoadContext parentLoadCtx = AssemblyLoadContext
                    .GetLoadContext(typeof(HostingStartupHostBuilderExtensions).Assembly)
                    ?? AssemblyLoadContext.Default;
                StringComparer cmp = StringComparer.OrdinalIgnoreCase;
                List<AssemblyName> excludeAssembyNames = [
                    .. ParseSemicolonSeparatedList(dynamicConfig[HostingStartupDefaults.HostingStartupExcludeAssembliesKey])
                        .Distinct(cmp)
                        .Select(nameString => new AssemblyName(nameString))
                    ];
                HashSet<string> loadAssemblies = new(
                    ParseSemicolonSeparatedList(dynamicConfig[HostingStartupDefaults.HostingStartupAssembliesKey]),
                    cmp
                    );

                foreach (var assemblyName in loadAssemblies.Select(n => new AssemblyName(n)))
                {
                    HostingStartupAssemblyLoadContext assemblyCtx = new(
                        name: $"HostingStartup: {assemblyName}",
                        path: AppContext.BaseDirectory,
                        parentLoadCtx,
                        excludeAssembyNames
                        );
                    Assembly assembly = assemblyCtx.LoadFromAssemblyName(assemblyName);
                    ProcessHostingStartupAssembly(assembly, hostBuilder);
                }

                loadAssemblies = new(
                    ParseSemicolonSeparatedList(dynamicConfig[HostingStartupDefaults.HostingStartupAssemblyFilePathsKey]),
                    cmp
                    );
                foreach (var assemblyFilePath in loadAssemblies)
                {
                    HostingStartupAssemblyLoadContext assemblyCtx = new(
                        name: $"HostingStartup: {assemblyFilePath}",
                        path: Path.GetDirectoryName(assemblyFilePath)
                            ?? AppContext.BaseDirectory,
                        parentLoadCtx,
                        excludeAssembyNames
                        );
                    var assemblyName = AssemblyName.GetAssemblyName(assemblyFilePath);
                    if (excludeAssembyNames.Any(GetMatcherFunc(assemblyName)))
                        continue;
                    Assembly assembly = assemblyCtx.LoadFromAssemblyPath(assemblyFilePath);
                    ProcessHostingStartupAssembly(assembly, hostBuilder);
                }
            }
            finally
            {
                (dynamicConfig as IDisposable)?.Dispose();
            }
        });
    }

    internal static Func<AssemblyName, bool> GetMatcherFunc(AssemblyName assemblyName)
        => assemblyNameCandidate => AssemblyName.ReferenceMatchesDefinition(
            assemblyNameCandidate,
            assemblyName
            );

    private static void ProcessHostingStartupAssembly(
        Assembly assembly,
        IHostBuilder hostBuilder
        )
    {
        IEnumerable<HostingStartupAttribute> hostingStartupMarkers =
            assembly.GetCustomAttributes<HostingStartupAttribute>();
        foreach (var hostingStartupMarker in hostingStartupMarkers)
        {
            Type hostingStartupType = hostingStartupMarker.HostingStartupType;
            var hostingStartupInstance =
                Activator.CreateInstance(hostingStartupType)
                as IHostingStartup;
            hostingStartupInstance?.Configure(hostBuilder);
        }
    }

    private static bool? ParseBooleanLikeValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        if (bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (bool.FalseString.Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int parsedInt))
        {
            switch (parsedInt)
            {
                case 0:
                    return false;
                case 1:
                    return true;
            }
        }

        return null;
    }

    private static string[] ParseSemicolonSeparatedList(string? value)
    {
        const StringSplitOptions splitOpts =
            StringSplitOptions.TrimEntries |
            StringSplitOptions.RemoveEmptyEntries;
        return value?.Split(';', splitOpts)
            ?? [];
    }
}