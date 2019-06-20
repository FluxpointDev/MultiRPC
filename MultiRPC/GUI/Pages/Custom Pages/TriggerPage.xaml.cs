using MultiRPC.JsonClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Microsoft.Win32;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for TriggerPage.xaml
    /// </summary>
    public partial class TriggerPage : Page
    {
        public static TriggerPage _TriggerPage;

        public TriggerPage(double pageWidth)
        {
            InitializeComponent();

            Width = pageWidth;
            _TriggerPage = this;
            btnMonday.Tag = DayOfWeek.Monday;
            btnTuesday.Tag = DayOfWeek.Tuesday;
            btnWednesday.Tag = DayOfWeek.Wednesday;
            btnThursday.Tag = DayOfWeek.Thursday;
            btnFriday.Tag = DayOfWeek.Friday;
            btnSaturday.Tag = DayOfWeek.Saturday;
            btnSunday.Tag = DayOfWeek.Sunday;
        }

        private void DayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Background == App.Current.Resources["AccentColour1SCBrush"])
            {
                MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Time.Days.Add((DayOfWeek)((Button)sender).Tag);
                ((Button)sender).SetResourceReference(ButtonBase.BackgroundProperty, "AccentColour2SCBrush");
            }
            else
            {
                MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Time.Days.Remove((DayOfWeek)((Button)sender).Tag);
                ((Button)sender).SetResourceReference(ButtonBase.BackgroundProperty, "AccentColour1SCBrush");
            }

            MasterCustomPage.SaveProfiles();
        }

        private string GetNumber(CustomProfile profile, DayOfWeek dayOfWeek)
        {
            return profile.Triggers.Time.Days.Contains(dayOfWeek) ? "2" : "1";
        }

        public async void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = MasterCustomPage.Profiles[((Button)sender).Content.ToString()];

            btnMonday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Monday)}SCBrush");
            btnTuesday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Tuesday)}SCBrush");
            btnWednesday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Wednesday)}SCBrush");
            btnThursday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Thursday)}SCBrush");
            btnFriday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Friday)}SCBrush");
            btnSaturday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Saturday)}SCBrush");
            btnSunday.SetResourceReference(Control.BackgroundProperty, $"AccentColour{GetNumber(profile, DayOfWeek.Sunday)}SCBrush");

            txtTimerLength.Text =
                $"{profile.Triggers.TimerLength.Hours:00}:{profile.Triggers.TimerLength.Minutes:00}:{profile.Triggers.TimerLength.Seconds:00}";
            txtEndTime.Text =
                $"{profile.Triggers.Time.EndTime.Hours:00}:{profile.Triggers.Time.EndTime.Minutes:00}:{profile.Triggers.Time.EndTime.Seconds:00}";
            txtStartTime.Text =
                $"{profile.Triggers.Time.StartTime.Hours:00}:{profile.Triggers.Time.StartTime.Minutes:00}:{profile.Triggers.Time.StartTime.Seconds:00}";

            txtFolderLocation.Text = profile.Triggers.FolderChange;
            txtFileLocation.Text = profile.Triggers.FileChange;
            txtProcessLocation.Text = profile.Triggers.Process;
        }

        private void TimespanLogic(string time, Action<TimeSpan> editLogic, TextBlock errorTbl)
        {
            if (MasterCustomPage.CurrentButton == null) return;

            if (time.Length == 0)
            {
                editLogic(TimeSpan.Zero);
                errorTbl.Visibility = Visibility.Collapsed;
                MasterCustomPage.SaveProfiles();
                return;
            }

            if (TimeSpan.TryParseExact(time, "h\\:mm\\:ss", new NumberFormatInfo(), out TimeSpan span))
            {
                editLogic(span);
                errorTbl.Visibility = Visibility.Collapsed;
                MasterCustomPage.SaveProfiles();
            }
            else
            {
                errorTbl.Visibility = Visibility.Visible;
            }
        }

        private void FileFolderLogic(string text, Action<string> editLogic, TextBlock errorTbl, bool lookingAtFolder, bool needsToBeExe = false)
        {
            if (text.Length == 0)
            {
                editLogic(text);
                errorTbl.Visibility = Visibility.Collapsed;
                MasterCustomPage.SaveProfiles();
                return;
            }

            if (needsToBeExe || !lookingAtFolder && File.Exists(text) || lookingAtFolder && Directory.Exists(text))
            {
                if (needsToBeExe && text.Split('.').Last() != "exe")
                {
                    errorTbl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (needsToBeExe)
                    {
                        text = text.Remove(0, text.LastIndexOf("\\") + 1);
                    }
                    editLogic(text);
                    errorTbl.Visibility = Visibility.Collapsed;
                    MasterCustomPage.SaveProfiles();
                }
            }
            else
            {
                errorTbl.Visibility = Visibility.Visible;
            }
        }


        private void TxtTimerLength_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TimespanLogic(
            txtTimerLength.Text, 
            (timeSpan) =>
            {
                MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.TimerLength = timeSpan;
            }, 
            tblTimerInvalid);
        }

        private void TxtEndTime_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TimespanLogic(
                txtEndTime.Text,
                (timeSpan) =>
                {
                    MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Time.EndTime = timeSpan;
                },
                tblEndTimeInvalid);
        }

        private void TxtStartTime_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TimespanLogic(
                txtStartTime.Text,
                (timeSpan) =>
                {
                    MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Time.StartTime = timeSpan;
                },
                tblStartTimeInvalid);
        }

        private void TxtFileName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FileFolderLogic(txtFileLocation.Text,
            (s) =>
            {
                MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.FileChange = s;
            },
            tblCanNotGetFindFile, 
            false);
        }

        private void TxtFolderName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FileFolderLogic(txtFolderLocation.Text,
                (s) =>
                {
                    MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.FolderChange = s;
                },
                tblCanNotGetFindFolder,
                true);
        }

        private void BtnFolderName_OnClick(object sender, RoutedEventArgs e)
        {
            //Find a better way of getting Folder
            var openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = false;
            openFileDialog.ValidateNames = false;
            openFileDialog.FileName = "Don't Edit this part";
            if (openFileDialog.ShowDialog(App.Current.MainWindow).Value)
            {
                txtFolderLocation.Text = openFileDialog.FileName.Remove(openFileDialog.FileName.LastIndexOf('\\'));
            }
        }

        private void BtnFileName_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileOk += OpenFileDialog_FileOk;
            if (openFileDialog.ShowDialog(App.Current.MainWindow).Value)
            {
                txtFileLocation.Text = openFileDialog.FileName;
            }
        }

        private void CboProcess_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                try
                {
                    var processLocation = ((Process) e.AddedItems[0]).MainModule?.FileName;
                    txtProcessLocation.Text = processLocation;
                }
                catch (Exception exception)
                {
                    App.Logging.Error("Application", exception);
                    tblCanNotGetProcessLocation.Visibility = Visibility.Visible;
                }
            }
        }

        private void CboProcess_OnDropDownOpened(object sender, EventArgs e)
        {
            //This for some reason this doesn't get some of them :thonk:
            var processName = new List<string>();
            var processes = new List<Process>(Process.GetProcesses().AsEnumerable());
            for (int i = 0; i < processes.LongCount(); i++)
            {
                if (processName.Contains(processes[i].ProcessName))
                {
                    processes.Remove(processes[i]);
                }
                else
                {
                    processName.Add(processes[i].ProcessName);
                }
            }
            cboProcess.ItemsSource = processes;
        }

        private void BtnSelectExe_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Programs | *.exe";
            if (openFileDialog.ShowDialog(App.Current.MainWindow).Value)
            {
                txtProcessLocation.Text = openFileDialog.FileName.Split('\\').Last();
            }
        }

        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var openFileDialog = (OpenFileDialog) sender;
            if (openFileDialog.FileName.Split('.').Last() == "exe")
            {
                CustomMessageBox.Show("You can't check on a exe");
                e.Cancel = true;
            }
        }

        private void TxtProcessLocation_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FileFolderLogic(txtProcessLocation.Text,
                (s) =>
                {
                    MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Process = s;
                },
                tblCanNotGetProcessLocation,
                false, 
                true);
        }
    }
}
