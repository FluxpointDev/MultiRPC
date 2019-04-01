using System;
using MultiRPC.GUI;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using MultiRPC.GUI.Controls;
using MultiRPC.GUI.Pages;
using MultiRPC.JsonClasses;

namespace MultiRPC.Functions
{
    public static class Updater
    {
        public static bool IsChecking;
        public static bool IsUpdating;
        public static bool BeenUpdated;

        public static async Task Check(bool showNoUpdateMessage = false)
        {
            if (BeenUpdated)
                return;

            IsChecking = true;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                try
                {
                    MainPage.mainPage.Dispatcher.Invoke(() =>
                    {
                        MainPage.mainPage.pbUpdateProgress.Visibility = Visibility.Visible;
                        MainPage.mainPage.pbUpdateProgress.IsIndeterminate = true;
                        MainPage.mainPage.pbUpdateProgress.ToolTip = new ToolTip($"{App.Text.CheckingForUpdates}...");
                    });
                    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                        App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

                    UpdateCheckInfo Info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);

                    MainPage.mainPage.Dispatcher.Invoke(() =>
                    {
                        MainPage.mainPage.pbUpdateProgress.IsIndeterminate = false;
                        MainPage.mainPage.pbUpdateProgress.ToolTip = null;
                    });
                    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                        App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

                    if (Info.UpdateAvailable)
                    {
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                await client.DownloadFileTaskAsync("https://multirpc.blazedev.me/Changelog.txt",
                                    FileLocations.ChangelogFileLocalLocation);
                            }
                        }
                        catch
                        {

                        }

                        if (App.Config.AutoUpdate)
                            Start();
                        else
                        {
                            var tick = DateTime.Now.Ticks;
                            await App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                var page = new UpdatePage(Info, tick);
                                var window = new MainWindow(page, false);
                                window.WindowID = tick;
                                window.ShowDialog();
                                if (window.ToReturn != null)
                                {
                                    IsChecking = false;
                                }
                            });

                            IsChecking = false;
                        }
                    }
                    else if (showNoUpdateMessage)
                    {
                        await CustomMessageBox.Show(App.Text.NoUpdate);
                    }
                }
                catch
                {
                    App.Logging.Error("App",App.Text.UpdateCheckFailed);
                    MainPage.mainPage.Dispatcher.Invoke(() =>
                    {
                        MainPage.mainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
                        MainPage.mainPage.pbUpdateProgress.IsIndeterminate = false;
                        MainPage.mainPage.pbUpdateProgress.ToolTip = null;
                    });
                    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                        App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                }
            }

            IsChecking = false;
        }


        public static async Task Start()
        {
            if (BeenUpdated)
                return;

            IsUpdating = true;
            MainPage.mainPage.Dispatcher.Invoke(() =>
            {
                MainPage.mainPage.pbUpdateProgress.Visibility = Visibility.Visible;
                MainPage.mainPage.pbUpdateProgress.IsIndeterminate = true;
                MainPage.mainPage.pbUpdateProgress.ToolTip = new ToolTip($"{App.Text.StartingUpdate}...");
            });
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            try
            {
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeployment_UpdateCompleted;
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += CurrentDeployment_UpdateProgressChanged;
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            catch(Exception e)
            {
                IsUpdating = false;
                MainPage.mainPage.Dispatcher.Invoke(() =>
                {
                    MainPage.mainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
                    MainPage.mainPage.pbUpdateProgress.IsIndeterminate = false;
                    MainPage.mainPage.pbUpdateProgress.ToolTip = null;
                });
                if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                    App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var tick = DateTime.Now.Ticks;
                    var window = new MainWindow(new UpdateFailedPage(e, tick), false);
                    window.WindowID = tick;
                    window.ShowDialog();
                });
            }
        }

        private static async void CurrentDeployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            string State = App.Text.Unknown;
            switch (e.State)
            {
                case DeploymentProgressState.DownloadingApplicationFiles:
                    State = App.Text.DownloadingFiles;
                    break;
                case DeploymentProgressState.DownloadingApplicationInformation:
                    State = App.Text.GetAppInfo;
                    break;
                case DeploymentProgressState.DownloadingDeploymentInformation:
                    State = App.Text.DownloadingDeploymentInfo;
                    break;
            }
            await MainPage.mainPage.Dispatcher.InvokeAsync(() =>
            {
                MainPage.mainPage.pbUpdateProgress.Value = e.ProgressPercentage;
                MainPage.mainPage.pbUpdateProgress.ToolTip = new ToolTip ($"{State} ({e.ProgressPercentage}%/100%)");
            });
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                App.Current.MainWindow.TaskbarItemInfo.ProgressValue = e.ProgressPercentage;
            }
        }

        private static async void CurrentDeployment_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            IsUpdating = false;
            if (e.Error == null)
            {
                BeenUpdated = true;
                if (App.Config.AutoUpdate)
                {
                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var tick = DateTime.Now.Ticks;
                        var window = new MainWindow(new UpdatePage(null, tick), false);
                        window.WindowID = tick;
                        window.ShowDialog();
                    });
                }
                else
                {
                    if (File.Exists(FileLocations.MultiRPCStartLink))
                        Process.Start(FileLocations.MultiRPCStartLink);
                    App.Current.Shutdown();
                }
            }
            else
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var tick = DateTime.Now.Ticks;
                    var window = new MainWindow(new UpdateFailedPage(e.Error, tick), false);
                    window.WindowID = tick;
                    window.ShowDialog();
                });
            }
            MainPage.mainPage.Dispatcher.Invoke(() =>
            {
                MainPage.mainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
                MainPage.mainPage.pbUpdateProgress.IsIndeterminate = false;
                MainPage.mainPage.pbUpdateProgress.ToolTip = null;
            });
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                App.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
        }
    }
}
