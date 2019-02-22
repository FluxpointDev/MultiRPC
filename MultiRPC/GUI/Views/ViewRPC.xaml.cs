using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace MultiRPC.GUI
{
    public enum ViewType
    {
        Default, Default2, Custom, Loading, Error, Update, UpdateFail
    }
    public partial class ViewRPC : UserControl
    {
        public ViewRPC(ViewType view, string Error = "")
        {
            InitializeComponent();
            switch (view)
            {
                case ViewType.Default:
                    {
                        Title.Content = "MultiRPC";
                        Text1.Content = "Thanks for using";
                        Text2.Content = "This program";
                        Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
                        SmallBack.Fill = (Brush)Application.Current.Resources["Brush_TabBackground"];

                    }
                    break;
                case ViewType.Default2:
                    {
                        Title.Content = "MultiRPC";
                        Text1.Content = "Hello";
                        Text2.Content = "World";
                        SmallBack.Visibility = Visibility.Hidden;
                        SmallImage.Fill = null;
                    }
                    break;
                case ViewType.Loading:
                    {
                        Title.Content = "Loading...";
                        Text1.Content = "";
                        Text2.Content = "";
                        BitmapImage image = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Loading.gif", UriKind.Absolute));
                        ImageBehavior.SetAnimatedSource(Loading, image);
                        ImageBehavior.SetRepeatBehavior(Loading, System.Windows.Media.Animation.RepeatBehavior.Forever);
                        LargeImage.Source = null;
                        SmallImage.Fill = null;
                        Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
                        Loading.Visibility = Visibility.Visible;
                        SmallBack.Visibility = Visibility.Hidden;
                    }
                    break;
                case ViewType.Update:
                    {
                        Title.Content = "Update";
                        Text1.Content = "Downloading...";
                        Text2.Content = "0%";
                        LargeImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/DownloadIcon.png", UriKind.Absolute));
                        SmallImage.Fill = null;
                        SmallBack.Visibility = Visibility.Hidden;
                    }
                    break;
                case ViewType.UpdateFail:
                    {
                        Title.Content = "Update";
                        Text1.Content = "Failed!";
                        Text2.Content = ":(";
                        LargeImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/DownloadIcon.png", UriKind.Absolute));
                        SmallImage.Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/ExitIcon.png", UriKind.Absolute)));
                        Background = new SolidColorBrush(new Color { R = 255, G = 57, B = 57, A = 80 });
                    }
                    break;
                case ViewType.Error:
                    {
                        Title.Content = "Error!";
                        Text1.Content = Error;
                        Text2.Content = "";
                        Background = new SolidColorBrush(new Color { R = 255, G = 57, B = 57, A = 80});
                        LargeImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/ExitIcon.png", UriKind.Absolute));
                        SmallImage.Fill = null;
                        SmallBack.Visibility = Visibility.Hidden;
                    }
                    break;
            }
        }

        public ViewRPC(DiscordRPC.Message.PresenceMessage msg)
        {
            InitializeComponent();
            Title.Content = msg.Name;
            Text1.Content = msg.Presence.Details;
            Text2.Content = msg.Presence.State;
            if (msg.Presence.HasAssets())
            {
                if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageKey))
                {
                    BitmapImage Small = new BitmapImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.SmallImageID + ".png"));
                    Small.DownloadFailed += Image_FailedLoading;
                    SmallImage.Fill = new ImageBrush(Small);
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageText))
                        SmallImage.ToolTip = new Button().Content = msg.Presence.Assets.SmallImageText;
                }
                else
                {
                    SmallImage.Fill = null;
                    SmallBack.Visibility = Visibility.Hidden;
                }
                if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageKey))
                {
                    LargeImage.Visibility = Visibility.Visible;
                    BitmapImage Large = new BitmapImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.LargeImageID + ".png"));
                    Large.DownloadFailed += Image_FailedLoading;
                    LargeImage.Source = Large;
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageText))
                        LargeImage.ToolTip = new Button().Content = msg.Presence.Assets.LargeImageText;
                }
                else
                    LargeImage.Source = null;
            }
        }

        public static void Image_FailedLoading(object sender, ExceptionEventArgs e)
        {
            App.Log.ImageError(sender as BitmapImage, e);
        }
    }
}
