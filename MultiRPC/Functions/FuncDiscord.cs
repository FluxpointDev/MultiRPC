using MultiRPC.GUI;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace MultiRPC.Functions
{
    public static class FuncDiscord
    {
        public static bool CheckDiscordClient()
        {
            try
            {
                LoadPipes();
                if (App.WD.Title == "MultiRPC")
                {
                    App.Log.Error("RPC", "No Discord client found");
                    MainWindow.EnableElements();
                    App.WD.FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "No Discord client found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error("RPC", $"No Discord client found, {ex.Message}");
                MainWindow.EnableElements();
                App.WD.FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "No Discord client");
                return false;
            }
            return true;
        }


        public static void LoadPipes()
        {
            bool Found = false;
            foreach (string p in Directory.GetFiles(@"\\.\pipe\"))
            {
                string Pipe = p.Replace(@"\\.\pipe\", "").Replace("-sock", "");
                if (Pipe == "discord")
                {
                    App.WD.Title = "MultiRPC - Discord";
                    Found = true;
                    break;
                }
                if (Pipe == "discordptb")
                {
                    App.WD.Title = "MultiRPC - Discord PTB";
                    Found = true;
                    break;
                }
                if (Pipe == "discordcanary")
                {
                    App.WD.Title = "MultiRPC - Discord Canary";
                    Found = true;
                    break;
                }
            }
            if (!Found)
                App.WD.Title = "MultiRPC";
        }

        public static bool CheckPortOld()
        {
            bool isAvailable = true;
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Address.ToString() == "127.0.0.1" && endpoint.Port > 6462 && endpoint.Port < 6473)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}
