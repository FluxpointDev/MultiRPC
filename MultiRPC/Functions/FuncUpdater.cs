using MultiRPC.GUI;
using MultiRPC.GUI.Pages;
using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

namespace MultiRPC.Functions
{
    public static class FuncUpdater
    {
        public static void Check()
        {
            if (File.Exists(App.ConfigFolder + "Changelog.txt"))
            {
                using (StreamReader reader = new StreamReader(App.ConfigFolder + "Changelog.txt"))
                {
                    App.Changelog = reader.ReadToEnd();
                }
            }
            else
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile("https://multirpc.blazedev.me/Changelog.txt", App.ConfigFolder + "Changelog.txt");
                    }
                    using (StreamReader reader = new StreamReader(App.ConfigFolder + "Changelog.txt"))
                    {
                        App.Changelog = reader.ReadToEnd();
                    }
                }
                catch { }
            }

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                App.WD.TextVersion.Content = App.Version;
                UpdateCheckInfo Info = null;
                try
                {
                    Info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);
                    if (Info != null && Info.UpdateAvailable)
                    {
                        App.StartUpdate = true;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile("https://multirpc.blazedev.me/Changelog.txt", App.ConfigFolder + "Changelog.txt");
                            }
                            using (StreamReader reader = new StreamReader(App.ConfigFolder + "Changelog.txt"))
                            {
                                App.Changelog = reader.ReadToEnd();
                            }
                        }
                        catch { }
                        UpdateWindow UpdateWindow = new UpdateWindow
                        {
                            Info = Info
                        };
                        UpdateWindow.ShowDialog();
                        if (App.StartUpdate)
                        {
                            Start();
                        }
                    }
                }
                catch
                {
                    App.Log.Error("App", "Failed to check for updates");
                }
            }
        }

        
        public static void Start()
        {
            ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeployment_UpdateCompleted;
            //ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += CurrentDeployment_UpdateProgressChanged;
           
            ApplicationDeployment.CurrentDeployment.UpdateAsync();
        }

        private static void CurrentDeployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            string State = "Unknown?";
            switch(e.State)
            {
                case DeploymentProgressState.DownloadingApplicationFiles:
                    State = "Downloading files";
                    break;
                case DeploymentProgressState.DownloadingApplicationInformation:
                    State = "Getting app info";
                    break;
                case DeploymentProgressState.DownloadingDeploymentInformation:
                    State = "Downloading deployment";
                    break;
            }
           App.BW.LabelLoading.Content = $"{State} {e.ProgressPercentage}%/100%";
        }

        private static void CurrentDeployment_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/MultiRPC.appref-ms";
                if (File.Exists(filepath))
                    Process.Start(filepath);
                Application.Current.Shutdown();
            }
            else
            {
                ErrorWindow ErrorWindow = new ErrorWindow();
                ErrorWindow.SetUpdateError(e.Error);
                ErrorWindow.ShowDialog();
                Application.Current.Shutdown();
            }
        }
    }
}
