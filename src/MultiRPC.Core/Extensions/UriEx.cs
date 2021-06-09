using JetBrains.Annotations;
using System;
using System.Diagnostics;

namespace MultiRPC.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Uri"/>
    /// </summary>
    public static class UriEx
    {
        /// <summary>
        /// Opens the website in a web browser
        /// </summary>
        /// <param name="uriString">website to open</param>
        public static void OpenWebsite(this string uriString)
        {
            Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri);
            uri?.OpenWebsite();
        }

        /// <summary>
        /// Opens the website in a web browser
        /// </summary>
        /// <param name="uri">website to open</param>
        public static void OpenWebsite([NotNull] this Uri uri)
        {
#if NETCOREAPP
            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}")
            {
                CreateNoWindow = true
            });
#else
            Process.Start(uri.AbsoluteUri);
#endif
        }
    }
}
