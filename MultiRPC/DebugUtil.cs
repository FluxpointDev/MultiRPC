using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MultiRPC
{
    public class DebugUtil
    {
        public static bool IsDebugBuild { get; } = IsAssemblyDebugBuild(Assembly.GetExecutingAssembly());

        //https://stackoverflow.com/a/2186634
        public static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);
        }
    }
}