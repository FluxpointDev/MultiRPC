using JetBrains.Annotations;
using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace MultiRPC.Core
{
    /// <summary>
    /// Some small but useful logic
    /// </summary>
    public static class Utils
    {
        static Utils()
        {
            GetOSPlatform();
        }

        /// <summary>
        /// Gets if the users device has a network connection
        /// </summary>
        public static bool NetworkIsAvailable()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (var i = 0; i < networkInterfaces.LongLength; i++)
            {
                var item = networkInterfaces[i];
                if (item.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                if (item.Name.ToLower().Contains("virtual") || item.Description.ToLower().Contains("virtual"))
                {
                    continue; //Exclude virtual networks set up by VMWare and others
                }

                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    return true;
                }
            }

            return false;
        }

        //TODO: Find out how to get the status for other OS's
        /// <summary>
        /// Get if the client is running as an administrator.
        /// </summary>
        public static bool RunningAsAdministrator => OSPlatform == OSPlatform.Windows ?
           new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) : false;

        /// <summary>
        /// Gets what OS the user is running
        /// </summary>
        [NotNull]
        public static OSPlatform OSPlatform { get; private set; }

        private static async Task GetOSPlatform() 
        {
            OSPlatform plat;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                plat = OSPlatform.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                plat = OSPlatform.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                plat = OSPlatform.OSX;
            }

            if (plat == null)
            {
                throw new Exception(await LanguagePicker.GetLineFromLanguageFile("CanNotGetOS"));
            }
            OSPlatform = plat;
        }
    }
}
