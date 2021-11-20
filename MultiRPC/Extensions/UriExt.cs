using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TinyUpdate.Core.Helper;

namespace MultiRPC.Extensions
{
    public static class UriExt
    {
        public static bool OpenInBrowser(this Uri url) => OpenInBrowser(url.AbsoluteUri);

        //https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        public static bool OpenInBrowser(this string url)
        {
            try
            {
                Process.Start(url);
                return true;
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    return true;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                    return true;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                    return true;
                }
            }

            return false;
        }
    }
}