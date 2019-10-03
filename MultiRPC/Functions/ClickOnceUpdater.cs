using MultiRPC.GUI;
using MultiRPC.GUI.Controls;
using MultiRPC.GUI.Pages;
using MultiRPC.JsonClasses;
using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Extra;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace MultiRPC.Functions
{
    class ClickOnceUpdater : IUpdate
    {
        public bool IsChecking { get; private set; }
        public bool IsUpdating { get; private set; }
        public bool BeenUpdated { get; private set; }
        public string NewVersion { get; private set; }

        public async Task Check(bool showNoUpdateMessage = false)
        {
            if (BeenUpdated)
            {
                return;
            }

            IsChecking = true;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                try
                {
                    MainPage._MainPage.Dispatcher.Invoke(() =>
                    {
                        MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Visible;
                        MainPage._MainPage.pbUpdateProgress.IsIndeterminate = true;
                        MainPage._MainPage.pbUpdateProgress.ToolTip = new ToolTip($"{App.Text.CheckingForUpdates}...");
                    });
                    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                    {
                        Application.Current.MainWindow.TaskbarItemInfo.ProgressState =
                            TaskbarItemProgressState.Indeterminate;
                    }

                    var info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);

                    MainPage._MainPage.Dispatcher.Invoke(() =>
                    {
                        MainPage._MainPage.pbUpdateProgress.IsIndeterminate = false;
                        MainPage._MainPage.pbUpdateProgress.ToolTip = null;
                    });
                    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                    {
                        Application.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    }

                    if (info.UpdateAvailable)
                    {
                        NewVersion = info.AvailableVersion.ToString();
                        try
                        {
                            using var client = new WebClient();
                            await client.DownloadFileTaskAsync(
new[] { App.MultiRPCWebsiteRoot, "Changelog.txt" }.CombineToUri(),
FileLocations.ChangelogFileLocalLocation);
                        }
                        catch
                        {
                            App.Logging.Application($"{App.Text.CouldntDownload} {App.Text.Changelog}");
                        }

                        if (App.Config.AutoUpdate)
                        {
                            Update();
                        }
                        else
                        {
                            var tick = DateTime.Now.Ticks;
                            await Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                if (await MainWindow.OpenWindow(new UpdatePage(NewVersion, tick), true, tick, false) != null)
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
                    App.Logging.Error("App", App.Text.UpdateCheckFailed);
                    MainPage._MainPage.Dispatcher.Invoke(() =>
                    {
                        MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
                        MainPage._MainPage.pbUpdateProgress.IsIndeterminate = false;
                        MainPage._MainPage.pbUpdateProgress.ToolTip = null;
                    });
                    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                    {
                        Application.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    }
                }
            }

            IsChecking = false;
        }

        public async Task Update()
        {
            if (BeenUpdated)
            {
                return;
            }

            IsUpdating = true;
            MainPage._MainPage.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Visible;
                MainPage._MainPage.pbUpdateProgress.IsIndeterminate = true;
                MainPage._MainPage.pbUpdateProgress.ToolTip = new ToolTip($"{App.Text.StartingUpdate}...");
            });
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                Application.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            }

            try
            {
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeployment_UpdateCompleted;
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged +=
                    CurrentDeployment_UpdateProgressChanged;
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            catch (Exception e)
            {
                IsUpdating = false;
                MainPage._MainPage.Dispatcher.Invoke(() =>
                {
                    MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
                    MainPage._MainPage.pbUpdateProgress.IsIndeterminate = false;
                    MainPage._MainPage.pbUpdateProgress.ToolTip = null;
                });
                if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                {
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                }

                var tick = DateTime.Now.Ticks;
                await MainWindow.OpenWindow(new UpdateFailedPage(e, tick), true, tick, false);
            }
        }

        private static async void CurrentDeployment_UpdateProgressChanged(object sender,
        DeploymentProgressChangedEventArgs e)
        {
            var state = App.Text.Unknown;
            switch (e.State)
            {
                case DeploymentProgressState.DownloadingApplicationFiles:
                    state = App.Text.DownloadingFiles;
                    break;
                case DeploymentProgressState.DownloadingApplicationInformation:
                    state = App.Text.GetAppInfo;
                    break;
                case DeploymentProgressState.DownloadingDeploymentInformation:
                    state = App.Text.DownloadingDeploymentInfo;
                    break;
            }

            await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
            {
                MainPage._MainPage.pbUpdateProgress.Value = e.ProgressPercentage;
                MainPage._MainPage.pbUpdateProgress.ToolTip = new ToolTip($"{state} ({e.ProgressPercentage}%/100%)");
            });
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                Application.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                Application.Current.MainWindow.TaskbarItemInfo.ProgressValue = e.ProgressPercentage;
            }
        }

        private async void CurrentDeployment_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IsUpdating = false;
            if (e.Error == null)
            {
                BeenUpdated = true;
                if (App.Config.AutoUpdate)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var tick = DateTime.Now.Ticks;
                        MainWindow.OpenWindow(new UpdatePage(NewVersion, tick), true, tick, false);
                    });
                }
                else
                {
                    if (File.Exists(FileLocations.MultiRPCStartLink))
                    {
                        Process.Start(FileLocations.MultiRPCStartLink, "--fromupdate");
                    }

                    Application.Current.Shutdown();
                }
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var tick = DateTime.Now.Ticks;
                    MainWindow.OpenWindow(new UpdateFailedPage(e.Error, tick), true, tick, false);
                });
            }

            MainPage._MainPage.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
                MainPage._MainPage.pbUpdateProgress.IsIndeterminate = false;
                MainPage._MainPage.pbUpdateProgress.ToolTip = null;
            });
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                Application.Current.MainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }
        }
    }
}
