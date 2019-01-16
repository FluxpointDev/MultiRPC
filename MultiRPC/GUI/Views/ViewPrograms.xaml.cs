using MultiRPC.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ViewPrograms.xaml
    /// </summary>
    public partial class ViewPrograms : UserControl
    {
        public ViewPrograms()
        {
            InitializeComponent();
            //Programs.Add("afk", new Afk("AFK", "469643793851744257", ""));
            //Programs.Add("windows", new Windows("Windows", "469675182802599936", ""));
            //Programs.Add("anime", new Anime("Anime", "451178426439565312", ""));
            //Programs.Add("firefox", new Firefox("Firefox", "450894077165043722", "firefox"));
            //Programs.Add("chrome", new Chrome("Chrome", "", "chrome"));
            //Programs.Add("minecraft", new Minecraft("Minecraft", "", ""));
            //Programs.Add("winmedia", new WindowsMediaPlayer("Win Media Player", "450910667331993601", ""));
            //Programs.Add("custom", new Custom("Custom", "", ""));

            //   foreach (IProgram P in Data.Programs.Values.OrderBy(x => x.Data.Priority).Reverse())
            //  {
            //     Button B = new Button
            //      {
            //          Content = P.Name
            //      };
            //     B.Click += B_Click;
            //     if (P.Auto)
            //         B.Foreground = SystemColors.HotTrackBrush;
            //     else
            //        B.Foreground = SystemColors.ActiveCaptionTextBrush;
            //    List_Programs.Items.Add(B);
            //    Log.Program($"Loaded {P.Name}: {P.Data.Enabled} ({P.Data.Priority})");
            // }
        }
    }
}
