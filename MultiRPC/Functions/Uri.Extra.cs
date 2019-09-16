using MultiRPC;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;

namespace System.Extra
{
    public static class Uri
    {
        public static string Combine(this string[] strings)
        {
            var uri = "";
            for (var i = 0; i < strings.Length; i++)
            {
                uri += $"{strings[i]}/";
            }

            return uri.Remove(uri.Length - 1);
        }

        public static System.Uri CombineToUri(this string[] strings)
        {
            return new System.Uri(strings.Combine());
        }

        public static void OpenWebsite(this System.Uri uri)
        {
            uri.AbsoluteUri.OpenWebsite();
        }

        public static void OpenWebsite(this string uri)
        {
#if NETCORE
            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}")
            {
                CreateNoWindow = true
            });
#else
            Process.Start(uri);
#endif
        }

        public static List<string> GetQueryStringParameters()
        {
#if !NETCORE
            try
            {
                if (Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    var nameValueTable = HttpUtility.ParseQueryString(Deployment.Application.ApplicationDeployment.CurrentDeployment.ActivationUri.Query);
                    var list = new List<string>();
                    foreach (var item in nameValueTable.AllKeys)
                    {
                        list.Add(item);
                        var itemContent = nameValueTable.GetValues(item);
                        if (itemContent != null)
                        {
                            list.AddRange(itemContent);
                        }
                    }

                    return list.Count > 0 ? list : null;
                }
            }
            catch (Exception e)
            {
                App.Logging.Error("", e);
            }
#endif
            return null;
        }
    }
}