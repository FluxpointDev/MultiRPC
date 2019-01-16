using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MultiRPC.Functions
{
    public static class FuncCredits
    {
        public static CreditsList CreditsList = null;
        public static void Download()
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadFileAsync(new Uri("https://multirpc.blazedev.me/Credits.json"), App.ConfigFolder + "Credits.json");
            }
            catch { }
        }

        private static void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                using (StreamReader reader = new StreamReader(App.ConfigFolder + "Credits.json"))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    CreditsList = (CreditsList)serializer.Deserialize(reader, typeof(CreditsList));
                }
                App.WD.ListAdmins.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    App.WD.ListAdmins.Text = string.Join("\n\n", CreditsList.Admins);
                    App.WD.ListPatreon.Text = string.Join("\n\n", CreditsList.Patreon);
                    App.WD.ListPaypal.Text = string.Join("\n\n", CreditsList.Paypal);
                    CheckBadges();
                });
            }
            catch { }
        }
        public static void CheckBadges()
        {
            if (CreditsList.Admins.Contains(App.Config.LastUser))
            {
                App.WD.UserBadge.ToolTip = "Community Admin";
                App.WD.UserBadge.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/AdminBadge.png", UriKind.Absolute));
            }
            if (CreditsList.Patreon.Contains(App.Config.LastUser))
            {
                App.WD.UserBadge.ToolTip = "Patreon Donator";
                App.WD.UserBadge.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/DonatorBadge.png", UriKind.Absolute));
            }
            if (CreditsList.Paypal.Contains(App.Config.LastUser))
            {
                App.WD.UserBadge.ToolTip = "Paypal Donator";
                App.WD.UserBadge.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/DonatorBadge.png", UriKind.Absolute));
            }
        }

    }
    public class CreditsList
    {
        public List<string> Admins = new List<string>();
        public List<string> Patreon = new List<string>();
        public List<string> Paypal = new List<string>();
    }
}
