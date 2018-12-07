using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace MultiRPC.GUI
{
    public static class FuncUpdater
    {
        public static void Check()
        {
            if (File.Exists(RPC.ConfigFolder + "Changelog.txt"))
            {
                using (StreamReader reader = new StreamReader(RPC.ConfigFolder + "Changelog.txt"))
                {
                    App.WD.Changelog.Text = reader.ReadToEnd();
                }
            }
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Version Version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                App.Version = $"{Version.Major}.{Version.Minor}.{Version.Build}";
                App.WD.TextVersion.Content = App.Version;
                UpdateCheckInfo Info = null;
                try
                {
                    Info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);
                    if (Info != null && Info.UpdateAvailable)
                    {
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile("https://multirpc.blazedev.me/Changelog.txt", RPC.ConfigFolder + "Changelog.txt");
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
                            App.WD.ViewLiveRPC.Content = new ViewRPC(ViewType.Update);
                            Start();
                        }
                    }
                }
                catch
                {
                    RPC.Log.Error("App", "Failed to check for updates");
                }
            }
        }

        public static void Start()
        {
            ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeployment_UpdateCompleted;
            ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += CurrentDeployment_UpdateProgressChanged;
            ApplicationDeployment.CurrentDeployment.UpdateAsync();
        }

        private static void CurrentDeployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            (App.WD.ViewLiveRPC.Content as ViewRPC).Text2.Content = $"{e.ProgressPercentage}%/100%";
        }

        private static void CurrentDeployment_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/MultiRPC.appref-ms");
                Application.Current.Shutdown();
            }
            else
            {
                App.WD.ViewLiveRPC.Content = new ViewRPC(ViewType.UpdateFail);
                ErrorWindow ErrorWindow = new ErrorWindow();
                ErrorWindow.SetUpdateError(e.Error);
                ErrorWindow.ShowDialog();
                Application.Current.Shutdown();
            }
        }
    }
}
