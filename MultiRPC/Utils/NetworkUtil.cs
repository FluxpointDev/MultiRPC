using System.Net.NetworkInformation;

namespace MultiRPC.Utils
{
    public static class NetworkUtil
    {
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
    }
}