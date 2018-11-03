using System;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace MultiRPC.Programs
{
    public class WindowsMediaPlayer : IProgram
    {
        public WindowsMediaPlayer(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
            Auto = true;
        }
        [DllImport("User32.dll")]
        private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        public override void Update(DiscordRPC.RichPresence RP)
        {
      //      RP.largeImageKey = "winmedia";
       //     RP.largeImageText = "Windoware is best";
            IntPtr Handle = FindWindow("WMPlayerApp", "Windows Media Player");
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement Player = AutomationElement.FromHandle(Handle);
            AutomationElement AppHost = walker.GetFirstChild(Player);
            AutomationElement SkinHost = walker.GetFirstChild(AppHost);
            AutomationElement Con = walker.GetFirstChild(SkinHost);
            AutomationElement song = walker.GetFirstChild(Con);
            foreach (object i in song.CachedChildren)
            {
                Console.WriteLine(i);
            }

         //   DiscordRpc.UpdatePresence(RP);
        }
    }
}
