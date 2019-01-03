using MultiRPC.GUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace MultiRPC.Functions
{
    public static class FuncWindow
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
        
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int Length;
            public int Flags;
            public ShowWindowCommands ShowCmd;
            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        public static void CheckWindow(IntPtr handle)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.Length = Marshal.SizeOf(placement);
            GetWindowPlacement(handle, out placement);
            if (placement.ShowCmd == ShowWindowCommands.Hide)
                File.Create(RPC.ConfigFolder + "Open.rpc");
        }
    }

    public static class FileWatch
    {
        public static void Create()
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Filter = "*.rpc",
                Path = RPC.ConfigFolder,
                EnableRaisingEvents = true
            };
            watcher.Created += new FileSystemEventHandler(Watcher_FileCreated);
        }
        private static void Watcher_FileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "Open.rpc")
            {
                if (App.WD.Visibility == Visibility.Hidden)
                {
                    App.WD.Taskbar.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        try
                        {
                            App.WD.WindowState = WindowState.Normal;
                        }
                        catch { }
                        try
                        {
                            App.WD.Visibility = Visibility.Visible;
                        }
                        catch { }
                    });
                }
            }
        }
    }
}
