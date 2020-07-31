using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MultiRPC.GUI.Pages;
using Newtonsoft.Json;

namespace MultiRPC.JsonClasses
{
    public class CustomProfile
    {
        public string ClientID = "";
        public string LargeKey = "";
        public string LargeText = "";
        public string Name = "";
        public bool ShowTime = false;
        public string SmallKey = "";
        public string SmallText = "";
        public string Text1 = "";
        public string Text2 = "";
        public Trigger Triggers = new Trigger();
    }

    public class Trigger : INotifyPropertyChanged
    {
        private FileSystemWatcher? FileWatcher = null;
        private FileSystemWatcher? FolderWatcher = null;
        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private bool wasRPCRunningBeforeStarting;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (var t in Times)
            {
                t.ShowProfile += (_, __) => ShowProfile(true);
                t.StopShowingProfile += (_, __) => StopShowingProfile();
                if (t.IsActive)
                {
                    ShowProfile(true);
                }
            }
        }
        
        static Trigger() => RPC.RPCShutdown += (sender, args) =>
        {
            ActiveTriggers.Clear();
        };
        
        public Trigger()
        {
            foreach (var t in Times)
            {
                t.ShowProfile += (_, __) => ShowProfile(true);
                t.StopShowingProfile += (_, __) => StopShowingProfile();
                if (t.IsActive)
                {
                    ShowProfile(true);
                }
            }
        }
        
        private void SetupFolderWatcher(bool skipCheck = false)
        {
            if (!skipCheck && FolderWatcher != null)
            {
                return;
            }
            FolderWatcher = new FileSystemWatcher("..");
            
            FolderWatcher.Created += (sender, args) => ShowProfile();
            FolderWatcher.Changed += (sender, args) => ShowProfile();
            FolderWatcher.Deleted += (sender, args) => ShowProfile();
            FolderWatcher.Renamed += (sender, args) => ShowProfile();
            FolderWatcher.Error += (sender, args) => SetupFolderWatcher(true);
            
            SetFolderChange(IsFolderChangeUsable);
        }

        private void SetupFileWatcher(bool skipCheck = false)
        {
            if (!skipCheck && FileWatcher != null)
            {
                return;
            }

            FileWatcher = new FileSystemWatcher("..")
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.FileName |
                               NotifyFilters.LastWrite | NotifyFilters.Attributes
            };

            FileWatcher.Changed += (sender, args) => ShowProfile();
            FileWatcher.Renamed += (sender, args) =>
            {
                FileChange = args.Name;

                MasterCustomPage.SaveProfiles();
                ShowProfile();
            };
            FileWatcher.Error += (sender, args) => SetupFileWatcher(true);
            
            SetFileChange(IsFileChangeUsable);
        }
        
        public static List<Trigger> ActiveTriggers = new List<Trigger>();

        private string process = null;
        /// <summary>
        /// Trigger when a process is started/close
        /// </summary>
        public string Process
        {
            get => process;
            set
            {
                process = value;

                if (!string.IsNullOrWhiteSpace(process))
                {
                    ProcessWatcher.Start();
                }
                
                OnPropertyChanged();
            }
        }

        private string folderChange = "";
        /// <summary>
        /// Trigger when contents of this folder has changed
        /// </summary>
        public string FolderChange
        {
            get => folderChange;
            set
            {
                folderChange = value;

                var usable = IsFolderChangeUsable;
                if (usable)
                {
                    SetupFolderWatcher();
                }
                SetFolderChange(usable);

                OnPropertyChanged();
            }
        }

        private bool IsFolderChangeUsable => !string.IsNullOrWhiteSpace(FolderChange) && Directory.Exists(FolderChange);
        
        private void SetFolderChange(bool enable)
        {
            if (FolderWatcher == null) return;
            FolderWatcher.EnableRaisingEvents = enable;
            FolderWatcher.Path = FolderWatcher.EnableRaisingEvents ? FolderChange : "..";
        }
        
        private string fileChange = "";
        //FileWatcher.Filter = args.Name;
        /// <summary>
        /// Trigger for when contents of a file have been changed
        /// </summary>
        public string FileChange
        {
            get => fileChange;
            set
            {
                fileChange = value;
                
                var usable = IsFileChangeUsable;
                if (usable)
                {
                    SetupFileWatcher();
                }
                SetFileChange(usable);

                OnPropertyChanged();
            }
        }

        private bool IsFileChangeUsable => !string.IsNullOrWhiteSpace(FileChange) && File.Exists(FileChange);

        private void SetFileChange(bool enable)
        {
            if (FileWatcher == null) return;
            FileWatcher.EnableRaisingEvents = enable;
            FileWatcher.Path = FileWatcher.EnableRaisingEvents ? Path.GetDirectoryName(FileChange) : "..";
            FileWatcher.Filter = FileWatcher.EnableRaisingEvents ? Path.GetFileName(FileChange) : "..";
        }
        
        private TimeSpan timerLength = TimeSpan.Zero;

        /// <summary>
        /// Amount of time that this profile should show for
        /// </summary>
        public TimeSpan TimerLength
        {
            get => timerLength;
            set
            {
                timerLength = value;
                OnPropertyChanged();
            }
        }

        public DateTime? TimeTimerStarted = null;
        
        private bool isUsingTimer;
        [JsonIgnore]
        public bool IsUsingTimer
        {
            get => isUsingTimer;
            set
            {
                if (isUsingTimer == value)
                {
                    return;
                }
                
                isUsingTimer = value;

                if (isUsingTimer)
                {
                    RunTimer();
                }
                else
                {
                    CancellationTokenSource.Cancel();
                    CancellationTokenSource = new CancellationTokenSource();
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Times when this Profile should get triggered
        /// </summary>
        public ReadOnlyCollection<Day> Times = new ReadOnlyCollection<Day>(new List<Day>
        {
            new Day(DayOfWeek.Monday),
            new Day(DayOfWeek.Tuesday),
            new Day(DayOfWeek.Wednesday),
            new Day(DayOfWeek.Thursday),
            new Day(DayOfWeek.Friday),
            new Day(DayOfWeek.Saturday),
            new Day(DayOfWeek.Sunday),
        });

        public async Task ShowProfile(bool trackTrigger = false)
        {
            while (MasterCustomPage.Profiles == null)
            {
                await Task.Delay(1000);
            }
            
            //See if we are trying to use the same profile
            var profile = MasterCustomPage.Profiles.First(x => x.Value.Triggers == this).Value;
            if (RPC.Equals(RPC.Presence, profile) && RPC.IsRPCRunning)
            {
                return;
            }

            if (!await MultiRPCAndCustomLogic.CanRunRPC(profile.Text1, profile.Text2,
                profile.SmallText, profile.LargeText, profile.ClientID))
            {
                return;
            }

            if (trackTrigger)
            {
                ActiveTriggers.Add(this);
            }
            var clientID = ulong.Parse(profile.ClientID);
            
            var hadToShutdown = false;
            if (RPC.IDToUse != clientID)
            {
                RPC.Shutdown();
                hadToShutdown = true;
            }
            wasRPCRunningBeforeStarting = hadToShutdown;

            RPC.IDToUse = clientID;
            RPC.SetPresence(profile, true);
            if (hadToShutdown || !RPC.IsRPCRunning)
            {
                await RPC.Start();
            }
            else
            {
                RPC.Update();
            }
        }

        public async void StopShowingProfile()
        {
            ActiveTriggers.Remove(this);
            var profile = MasterCustomPage.Profiles.First(x => x.Value.Triggers == this).Value;
            var clientID = ulong.Parse(profile.ClientID);
            
            var hadToShutdown = false;
            if (RPC.IDToUse != clientID)
            {
                RPC.Shutdown();
                hadToShutdown = true;
            }

            RPC.IDToUse = clientID;
            if (ActiveTriggers.Count > 0)
            {
                RPC.SetPresence(MasterCustomPage.Profiles.First(x => x.Value.Triggers == ActiveTriggers[ActiveTriggers.Count - 1]).Value, true);
            }
            else
            {
                RPC.SetPresence(null);
            }
            
            if ((hadToShutdown || !RPC.IsRPCRunning) && wasRPCRunningBeforeStarting)
            {
                await RPC.Start();
            }
            else if (ActiveTriggers.Count == 0)
            {
                RPC.Shutdown();
            }
            else
            {
                RPC.Update();
            }
        }

        private async void RunTimer()
        {
            ShowProfile(true); 
            TimeTimerStarted = DateTime.Now;
            try
            {
                await Task.Delay(TimerLength, CancellationTokenSource.Token);
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e);
            }
            StopShowingProfile();
            IsUsingTimer = false;
            TimeTimerStarted = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum DayOfWeek
    {
        NotSet = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7,
    }
}