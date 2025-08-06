using System.Reflection;
using System.Runtime.Loader;

namespace FredrikHr.Extensions.HostingStartup;

internal sealed class HostingStartupAssemblyLoadContext(
    string name,
    string path,
    AssemblyLoadContext parent,
    IEnumerable<AssemblyName>? excludeAssemblies
    ) : AssemblyLoadContext(name)
{
    private readonly AssemblyDependencyResolver _resolver = new(path);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Func<AssemblyName, bool> assemblyNameMatcher =
            HostingStartupHostBuilderExtensions.GetMatcherFunc(assemblyName);
        if (excludeAssemblies?.Any(assemblyNameMatcher) ?? false)
            return null;

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (string.IsNullOrEmpty(path))
        {
            return parent.LoadFromAssemblyName(assemblyName);
        }

        AssemblyName nameFromPath = AssemblyName.GetAssemblyName(path);
        Func<AssemblyName, bool> assemblyPathMatcher =
            HostingStartupHostBuilderExtensions.GetMatcherFunc(nameFromPath);
        return excludeAssemblies?.Any(assemblyPathMatcher) ?? false
            ? null
            : LoadFromAssemblyPath(path);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        string? path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return string.IsNullOrEmpty(path) ? default : LoadUnmanagedDllFromPath(path);
    }
}
