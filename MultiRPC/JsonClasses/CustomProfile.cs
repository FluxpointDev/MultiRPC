using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        //TODO: Make it be able to handle when the user shutdowns the RPC Client 
        
        private FileSystemWatcher FileWatcher = new FileSystemWatcher("..");
        private FileSystemWatcher FolderWatcher = new FileSystemWatcher("..");
        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private bool wasRPCRunningBeforeStarting;
        
        public Trigger()
        {
            Times = new ReadOnlyCollection<Day>(new List<Day>
            {
                new Day(DayOfWeek.Monday),
                new Day(DayOfWeek.Tuesday),
                new Day(DayOfWeek.Wednesday),
                new Day(DayOfWeek.Thursday),
                new Day(DayOfWeek.Friday),
                new Day(DayOfWeek.Saturday),
                new Day(DayOfWeek.Sunday),
            });

            foreach (var t in Times)
            {
                t.ShowProfile += (_, __) => ShowProfile(true);
                t.StopShowingProfile += (_, __) => StopShowingProfile();
            }

            
            //TODO: Only do this when we got something to work with
            FolderWatcher.Created += (sender, args) => ShowProfile();
            FolderWatcher.Changed += (sender, args) => ShowProfile();
            FolderWatcher.Deleted += (sender, args) => ShowProfile();
            FolderWatcher.Renamed += (sender, args) => ShowProfile();
            FolderWatcher.Error += (sender, args) => ShowProfile(); //TODO: Re-setup

            FileWatcher.NotifyFilter = 
                NotifyFilters.LastAccess | NotifyFilters.FileName | 
                NotifyFilters.LastWrite | NotifyFilters.Attributes;
            FileWatcher.Changed += (sender, args) => ShowProfile();
            FileWatcher.Renamed += (sender, args) =>
            {
                FileChange = args.Name;

                MasterCustomPage.SaveProfiles();
                ShowProfile();
            };
            FileWatcher.Error += (sender, args) => ShowProfile(); //TODO: Re-setup
        }
        
        public static List<Trigger> ActiveTriggers = new List<Trigger>();
        
        /// <summary>
        /// Trigger when a process is started/close
        /// </summary>
        public string Process = null; //TODO

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

                FolderWatcher.EnableRaisingEvents = !string.IsNullOrWhiteSpace(FolderChange) && Directory.Exists(FolderChange);
                FolderWatcher.Path = FolderWatcher.EnableRaisingEvents ? value : "..";
                OnPropertyChanged();
            }
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

                FileWatcher.EnableRaisingEvents = !string.IsNullOrWhiteSpace(FileChange) && File.Exists(FileChange);
                FileWatcher.Path = FileWatcher.EnableRaisingEvents ? Path.GetDirectoryName(value) : "..";
                FileWatcher.Filter = FileWatcher.EnableRaisingEvents ? Path.GetFileName(value) : "..";
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Amount of time that this profile should show for
        /// </summary>
        public TimeSpan TimerLength = TimeSpan.Zero;

        //TODO: Use this as a way for the UI to track timing™
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
        public ReadOnlyCollection<Day> Times;

        public async void ShowProfile(bool trackTrigger = false)
        {
            //See if we are trying to use the same profile
            var profile = MasterCustomPage.Profiles.First(x => x.Value.Triggers == this).Value;
            if (RPC.Equals(RPC.Presence, profile) && RPC.IsRPCRunning)
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
                RPC.SetPresence(MasterCustomPage.Profiles.First(x => x.Value.Triggers == ActiveTriggers[^1]).Value, true);
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