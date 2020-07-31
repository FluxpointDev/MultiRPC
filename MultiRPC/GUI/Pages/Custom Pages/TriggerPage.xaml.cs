using MultiRPC.JsonClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using DayOfWeek = MultiRPC.JsonClasses.DayOfWeek;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for TriggerPage.xaml
    /// </summary>
    public partial class TriggerPage : Page
    {
        public static TriggerPage _TriggerPage;

        private static DayOfWeek ActiveDay = DayOfWeek.Monday;
        private static Button ActiveButton;
        private static CustomProfile ActiveProfile;
        private bool TimerLengthLogicRunning;
        
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
            
            Loaded += async (sender, args) =>
            {
                UpdateText();

                if (App.Config.HadTriggerWarning) return;
                await CustomMessageBox.Show(App.Text.TriggerWarning);
                App.Config.HadTriggerWarning = true;
                await App.Config.Save();
            };
        }

        public void UpdateText()
        {
            UpdateTimerButton();
            txtTime.Text = App.Text.Time;

            btnMonday.Content = App.Text.Mon;
            btnTuesday.Content = App.Text.Tu;
            btnWednesday.Content = App.Text.We;
            btnThursday.Content = App.Text.Th;
            btnFriday.Content = App.Text.Fr;
            btnSaturday.Content = App.Text.Sa;
            btnSunday.Content = App.Text.Su;

            tblStartTime.Text = $"{App.Text.StartTime}: ";
            tblEndTime.Text = $"{App.Text.EndTime}: ";
            tblStartTimeInvalid.Text = App.Text.TimeInvalid;
            tblEndTimeInvalid.Text = App.Text.TimeInvalid;
            tblTimerInvalid.Text = App.Text.TimeInvalid;
            txtTimer.Text = App.Text.Timer;
            tblLength.Text = $"{App.Text.Length}: ";
            btnFileLocation.Content = App.Text.SelectFile;
            tblFileChange.Text = App.Text.FileChange;
            tblFile.Text = $"{App.Text.File}: ";
            tblFolder.Text = $"{App.Text.Folder}: ";
            tblFolderChange.Text = App.Text.FolderChange;
            tblProcessOpenedActivated.Text = App.Text.ProcessOpenedActivated;
            tblProcess.Text = $"{App.Text.Process}: ";
            btnSelectExe.Content = App.Text.SelectExe;
            tblCanNotGetFindFile.Text = App.Text.CantFindFile;
            btnFolderLocation.Content = App.Text.SelectFolder;
            tblCanNotGetFindFolder.Text = App.Text.CantFindFolder;
            tblCanNotGetProcessLocation.Text = App.Text.CantFindExe;
        }

        private void UpdateTimerButton()
        {
            btnStartTimer.IsEnabled = ActiveProfile.Triggers.TimerLength != TimeSpan.Zero;
            btnStartTimer.Content = 
                MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.IsUsingTimer ? 
                    App.Text.StopTimer : App.Text.StartTimer;
        }
        
        private void DayButton_OnClick(object sender, RoutedEventArgs e)
        {
            var but = (Button) sender;
            var newActiveDay = (DayOfWeek) but.Tag;
            ActiveDay = newActiveDay;
            
            (ActiveButton ?? btnMonday).SetResourceReference(Control.BackgroundProperty, "AccentColour2SCBrush");
            ActiveButton = but;
            ActiveButton.SetResourceReference(Control.BackgroundProperty, "AccentColour1SCBrush");
            UpdateTimerTimes(MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Times.First(x => x.Date == ActiveDay));
        }

        private void UpdateTimerTimes(Day day)
        {
            txtEndTime.Text =
                $"{day.EndTime.Hours:00}:{day.EndTime.Minutes:00}:{day.EndTime.Seconds:00}";
            txtStartTime.Text =
                $"{day.StartTime.Hours:00}:{day.StartTime.Minutes:00}:{day.StartTime.Seconds:00}";
        }

        private void UpdateTime(object sender, EventArgs args) => 
            UpdateTimer(MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()]);
        
        public void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveProfile != null)
            {
                ActiveProfile.Triggers.PropertyChanged -= UpdateTime;
            }
            
            var profile = MasterCustomPage.Profiles[((Button)sender).Content.ToString()];
            ActiveProfile = profile;
            ActiveProfile.Triggers.PropertyChanged += UpdateTime;

            UpdateTimerButton();
            txtTimerLength.IsEnabled = !profile.Triggers.IsUsingTimer;

            UpdateTimerLength();
            UpdateTimerTimes(profile.Triggers.Times.First(x => x.Date == ActiveDay));

            txtFolderLocation.Text = profile.Triggers.FolderChange;
            txtFileLocation.Text = profile.Triggers.FileChange;
            txtProcessLocation.Text = profile.Triggers.Process;
        }

        private async Task UpdateTimerLength()
        {
            if (TimerLengthLogicRunning)
            {
                return;
            }
            TimerLengthLogicRunning = true;

            while (ActiveProfile.Triggers.IsUsingTimer && ActiveProfile.Triggers.TimeTimerStarted != null)
            {
                var time = ActiveProfile.Triggers.TimerLength
                    .Subtract(DateTime.Now.Subtract(ActiveProfile.Triggers.TimeTimerStarted.Value));
                txtTimerLength.Text =
                    $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
                await Task.Delay(1000);
            }
            txtTimerLength.Text =
                $"{ActiveProfile.Triggers.TimerLength.Hours:00}:{ActiveProfile.Triggers.TimerLength.Minutes:00}:{ActiveProfile.Triggers.TimerLength.Seconds:00}";
            TimerLengthLogicRunning = false;
        }
        
        private void TimespanLogic(string time, Action<TimeSpan> editLogic, TextBlock errorTbl)
        {
            if (MasterCustomPage.CurrentButton == null)
            {
                return;
            }

            if (time.Length == 0)
            {
                editLogic(TimeSpan.Zero);
                errorTbl.Visibility = Visibility.Collapsed;
                MasterCustomPage.SaveProfiles();
                return;
            }

            if (TimeSpan.TryParseExact(time, "h\\:mm\\:ss", new NumberFormatInfo(), out var span))
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
            if (TimerLengthLogicRunning)
            {
                return;
            }
            
            TimespanLogic(
            txtTimerLength.Text, 
            timeSpan =>
            {
                MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.TimerLength = timeSpan;
            }, 
            tblTimerInvalid);
        }

        private void TxtEndTime_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TimespanLogic(
                txtEndTime.Text,
                timeSpan =>
                {
                    MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Times.First(x => x.Date == ActiveDay).EndTime = timeSpan;
                },
                tblEndTimeInvalid);
        }

        private void TxtStartTime_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TimespanLogic(
                txtStartTime.Text,
                timeSpan =>
                {
                    MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].Triggers.Times.First(x => x.Date == ActiveDay).StartTime = timeSpan;
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
            var openFileDialog = new VistaFolderBrowserDialog();
            if (openFileDialog.ShowDialog(App.Current.MainWindow).Value)
            {
                txtFolderLocation.Text = openFileDialog.SelectedPath;
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
            if (e.AddedItems.Count == 0) return;
            
            try
            {
                var processLocation = ((Process)e.AddedItems[0])?.MainModule?.FileName;
                txtProcessLocation.Text = Path.GetFileName(processLocation);
                ProcessWatcher.Start();
            }
            catch (Exception exception)
            {
                App.Logging.Error("Application", exception);
                tblCanNotGetProcessLocation.Visibility = Visibility.Visible;
            }
        }

        private void CboProcess_OnDropDownOpened(object sender, EventArgs e)
        {
            //This for some reason this doesn't get some of them :thonk:
            var processesNames = new List<string>();
            var processes = new List<Process>(Process.GetProcesses().AsEnumerable());
            for (var i = 0; i < processes.LongCount(); i++)
            {
                var process = processes[i];
                if (processesNames.Contains(process.ProcessName))
                {
                    processes.Remove(process);
                    i--;
                }
                processesNames.Add(process.ProcessName);
            }
            cboProcess.ItemsSource = processes;
        }

        private void BtnSelectExe_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Programs | *.exe";
            if (openFileDialog.ShowDialog(App.Current.MainWindow).Value)
            {
                txtProcessLocation.Text = Path.GetFileName(openFileDialog.FileName);
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

        private void UpdateTimer(CustomProfile profile)
        {
            btnStartTimer.IsEnabled = profile.Triggers.TimerLength != TimeSpan.Zero;
            txtTimerLength.IsEnabled = !profile.Triggers.IsUsingTimer;
            UpdateTimerButton();
        }
        
        private void BtnStartTimer_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()];
            profile.Triggers.IsUsingTimer = btnStartTimer.Content == App.Text.StartTimer;
            UpdateTimerLength();
            UpdateTimer(profile);
        }
    }
}
