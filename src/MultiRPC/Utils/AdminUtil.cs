using System.Runtime.InteropServices;
using System.Security.Principal;
using TinyUpdate.Core.Helper;

namespace MultiRPC.Utils;

public static class AdminUtil
{
    public static bool IsAdmin { get; } = GetIsAdministrator();

    [DllImport("libc", SetLastError = true)]
    private static extern uint geteuid();

    private static bool GetIsAdministrator()
    {
        return TaskHelper.RunTaskBasedOnOS(() =>
            {
                if (!OperatingSystem.IsWindows()) return false;

                var windowsIdentity = WindowsIdentity.GetCurrent();
                var windowsPrincipal = new WindowsPrincipal(windowsIdentity);

                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }, () => geteuid() == 0, 
            () => geteuid() == 0);
    }
}