using System;
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
        public static void Start()
        {
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
            while (MasterCustomPage.Profiles == null)
            {
                await Task.Delay(1000);
            }
            
            while (true)
            {
                foreach (var process in Process.GetProcesses())
                {
                    var fileName = "";
                    try
                    {
                        fileName = process.MainModule?.FileName;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        continue;
                    }
                    var (_, profile) =
                        MasterCustomPage.Profiles.FirstOrDefault(x => x.Value.Triggers.Process == Path.GetFileName(fileName));

                    if (fileName == "7zFM.exe")
                    {
                        Debugger.Break();
                    }
                    
                    if (profile != null)
                    {
                        SetupTrigger(process, profile);
                        break;
                    }
                }
                
                await Task.Delay(App.Config.ProcessWaitingTime * 1000);
            }
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
            };
        }
        
        static void WaitForProcessAdmin()
        {
            try
            {
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

        static void NewProcess(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var proc = GetProcessInfo(e);
                var (_, profile) = MasterCustomPage.Profiles.FirstOrDefault(x => x.Value.Triggers.Process == proc.ProcessName);
                if (profile != null && proc.PID > -1)
                {
                    var process = Process.GetProcessById(proc.PID);
                    if (process != null)
                    {
                        SetupTrigger(process, profile);
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