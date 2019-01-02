using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Functions
{
    public static class FuncCredits
    {
        public static CreditsList CreditsList = null;
        public static void Download()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://multirpc.blazedev.me/Credits.json", RPC.ConfigFolder + "Credits.json");
                }
               
            }
            catch { }


        }
    }
    public class CreditsList
    {
        public List<string> Admins = new List<string>();
        public List<string> Patreon = new List<string>();
        public List<string> Paypal = new List<string>();
    }
}
