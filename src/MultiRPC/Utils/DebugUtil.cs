using System.Diagnostics;
using System.Reflection;

namespace MultiRPC.Utils;

public static class DebugUtil
{
    public static bool IsDebugBuild { get; } = IsAssemblyDebugBuild(Assembly.GetExecutingAssembly());

    //https://stackoverflow.com/a/2186634
    private static bool IsAssemblyDebugBuild(ICustomAttributeProvider assembly)
    {
        return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);
    }
}