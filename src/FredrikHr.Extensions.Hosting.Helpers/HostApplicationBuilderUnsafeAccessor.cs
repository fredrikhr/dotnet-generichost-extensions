#if NET9_0_OR_GREATER
using System.Runtime.CompilerServices;
#else
using System.Reflection;
#endif

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure
#pragma warning restore IDE0079 // Remove unnecessary suppression

internal static class HostApplicationBuilderUnsafeAccessor
{
#if NET9_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern IHostBuilder AsHostBuilder(HostApplicationBuilder builder);
#else
    private static readonly Lazy<MethodInfo> AsHostBuilderMethodInfo = new(
        () => typeof(HostApplicationBuilder).GetMethod(
            nameof(AsHostBuilder),
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic,
            Type.DefaultBinder,
            types: [],
            modifiers: default
            ) ?? throw new MissingMethodException(
                typeof(HostApplicationBuilder).FullName,
                nameof(AsHostBuilder)
            ));

    internal static IHostBuilder AsHostBuilder(HostApplicationBuilder builder)
    {
        try
        {
            return (IHostBuilder)AsHostBuilderMethodInfo.Value
                .Invoke(builder, parameters: null)!;
        }
        catch (TargetInvocationException targetInvokeExcept)
        when (targetInvokeExcept.InnerException is Exception innerExcept)
        {
            throw innerExcept;
        }
    }
#endif
}