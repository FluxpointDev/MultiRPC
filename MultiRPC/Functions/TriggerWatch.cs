//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Management;
//using System.Security.Principal;
//using System.Text;
//using System.Threading.Tasks;
//using MultiRPC.GUI.Pages;
//using MultiRPC.JsonClasses;

//namespace MultiRPC.Functions
//{
//    public static class TriggerWatch
//    {
//        private static Process[] _processes;
//        private static Process _process;

//        static TriggerWatch()
//        {
//            if (IsElevated)
//            {
//                var processStartEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
//                processStartEvent.EventArrived += ((sender, e) => ProcessStartEvent(sender, e.NewEvent.Properties["ProcessName"].Value.ToString()));
//                processStartEvent.Scope.Options.EnablePrivileges = true;

//                processStartEvent.Start();
//            }
//            else
//            {
//                _ = Task.Run(async () =>
//                {
//                    _processes = Process.GetProcesses();
//                    while (true)
//                    {
//                        var processes = Process.GetProcesses();
//                        if (processes.Length > _processes.Length)
//                        {
//                            List<string> processesNames = new List<string>(_processes.Length);
//                            for (int i = 0; i < _processes.LongLength; i++)
//                            {
//                                processesNames.Add(_processes[i].ProcessName);
//                            }
//                            for (int j = 0; j < processes.LongLength; j++)
//                            {
//                                if (!processesNames.Contains(processes[j].ProcessName))
//                                {
//                                    ProcessStartEvent(null, processes[j].ProcessName + ".exe");
//                                }
//                            }
//                        }
//                        _processes = Process.GetProcesses();
//                        await Task.Delay(100);
//                    }
//                });
//            }
//        }

//        private static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

//        #region Process
//        private static void ProcessStartEvent(object sender, string processName)
//        {
//            var processes = Process.GetProcesses();
//            for (int i = 0; i < MasterCustomPage.Profiles.Count; i++)
//            {
//                var profile = MasterCustomPage.Profiles.ElementAt(i).Value;

//                string btnStartContent = "";
//                if (MainPage._MainPage != null)
//                {
//                    MainPage._MainPage.Dispatcher.Invoke(() =>
//                    {
//                        if (MainPage._MainPage.btnStart != null)
//                        {
//                            btnStartContent = MainPage._MainPage.btnStart.Content.ToString();
//                        }
//                    });
//                }

//                App.Logging.Application(processName);
//                if (!RPC.Equals(RPC.Presence, profile) || (RPC.Equals(RPC.Presence, profile) && btnStartContent != App.Text.Shutdown) && processName == profile.Triggers.Process)
//                {
//                    for (int j = 0; j < processes.LongLength; j++)
//                    {
//                        App.Logging.Application(processes[j].ProcessName);
//                        if (processes[j].ProcessName + ".exe" == profile.Triggers.Process)
//                        {
//                            if (_process != null)
//                            {
//                                _process.Exited -= Process_Exited;
//                            }

//                            _process = processes[j];
//                            _process.EnableRaisingEvents = true;
//                            _process.Exited += Process_Exited;
//                            CustomPage.StartCustomProfileLogic(profile.Name);
//                            break;
//                        }
//                    }
//                    break;
//                }
//            }
//        }

//        private static void Process_Exited(object sender, EventArgs e)
//        {
//            for (int i = 0; i < MasterCustomPage.Profiles.Count; i++)
//            {
//                var profile = MasterCustomPage.Profiles.ElementAt(i).Value;

//                if (RPC.Equals(RPC.Presence, profile) && ((Process)sender).ProcessName + ".exe" == profile.Triggers.Process)
//                {
//                    RPC.Shutdown();
//                    break;
//                }
//            }
//        }
//        #endregion

//        /// <summary>
//        /// Just a void to trigger the static
//        /// </summary>
//        public static void Start()
//        {
//            return;
//        }
//    }
//}
