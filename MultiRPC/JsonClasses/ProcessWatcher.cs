using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using MultiRPC.GUI.Pages;

namespace MultiRPC.JsonClasses
{
    //Many thanks to https://stackoverflow.com/questions/38693623/truncated-processname-in-win32-processstarttrace-query
    //for pointing me to how to do things ♥
    public static class ProcessWatcher
    {
        private static bool Loaded = false;
        
        //Processes that we are waiting to close™
        private static List<string> ActiveProcesses = new List<string>();
        
        public static async Task Start()
        {
            if (Loaded)
            {
                return;
            }
            Loaded = true;
            
            while (MasterCustomPage.Profiles == null)
            {
                await Task.Delay(1000);
            }
            
            if (App.IsAdministrator)
            {
                WaitForProcessAdmin();
            }
            else
            {
                WaitForProcess();
            }
        }

        static async void WaitForProcess()
        {
            while (true)
            {
                foreach (var profile in 
                    MasterCustomPage.Profiles.Where(x => !string.IsNullOrEmpty(x.Value.Triggers.Process)))
                {
                    if (CheckProcesses(profile.Value))
                    {
                        break;
                    }
                }
                
                await Task.Delay(App.Config.ProcessWaitingTime * 1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        static void WaitForProcessAdmin()
        {
            try
            {
                //Check for anything
                foreach (var profile in
                    MasterCustomPage.Profiles.Where(x => !string.IsNullOrEmpty(x.Value.Triggers.Process)))
                {
                    if (CheckProcesses(profile.Value))
                    {
                        break;
                    }
                }
                
                var startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'"));
                startWatch.EventArrived += NewProcess;
                startWatch.Start();

                //var stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'"));
                //stopWatch.EventArrived += stopWatch_EventArrived;
                //stopWatch.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static bool CheckProcesses(CustomProfile profile)
        {
            if (ActiveProcesses.Contains(profile.Triggers.Process))
            {
                return false;
            }
            
            var openProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(profile.Triggers.Process));
            if (openProcesses.Length > 0)
            {
                SetupTrigger(openProcesses[0], profile);
                ActiveProcesses.Add(profile.Triggers.Process);
                return true;
            }

            return false;
        }

        static void SetupTrigger(Process process, CustomProfile profile)
        {
            //So we don't keep triggering it....
            if (Trigger.ActiveTriggers.Contains(profile.Triggers))
            {
                return;
            }
            
            profile.Triggers.ShowProfile(true);
            process.EnableRaisingEvents = true;
            process.Exited += (o, args) =>
            {
                profile.Triggers.StopShowingProfile();
                ActiveProcesses.Remove(profile.Triggers.Process);
            };
        }
        
        static void NewProcess(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var proc = GetProcessInfo(e);
                var profile = MasterCustomPage.Profiles.FirstOrDefault(x => x.Value.Triggers.Process == proc.ProcessName);
                if (profile.Value != null && proc.PID > -1)
                {
                    var process = Process.GetProcessById(proc.PID);
                    if (process != null)
                    {
                        SetupTrigger(process, profile.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static ProcessInfo GetProcessInfo(EventArrivedEventArgs e)
        {
            var p = new ProcessInfo();
            var prop = ((ManagementBaseObject) e.NewEvent["TargetInstance"]).Properties;
            var pid = -1;
            int.TryParse(prop["ProcessId"].Value.ToString(), out pid);
            p.PID = pid;
            p.ProcessName = prop["Name"].Value.ToString();

            return p;
        }

        private class ProcessInfo
        {
            public string? ProcessName { get; set; }
            public int PID { get; set; }
        }
    }
}