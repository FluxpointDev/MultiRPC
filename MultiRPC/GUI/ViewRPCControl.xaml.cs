using DiscordRPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace MultiRPC.GUI
{
    public enum ViewType
    {
        Default, Default2, Custom, Loading, Error
    }
    public class GifImage : Image
    {
        public string GifSource
        {
            get { return (string)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
                DependencyProperty.Register("GifSource", typeof(string),
                typeof(GifImage), new UIPropertyMetadata(null, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender,
                DependencyPropertyChangedEventArgs e)
        {
            (sender as GifImage).Initialize();
        }


        #region control the animate
        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        public bool IsAutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
                DependencyProperty.Register("IsAutoStart", typeof(bool),
                typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender,
                DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as GifImage).StartAnimation();
            else
                (sender as GifImage).StopAnimation();
        }
        #endregion

        private bool _isInitialized = false;
        private System.Drawing.Bitmap _bitmap;
        private BitmapSource _source;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource GetSource()
        {
            if (_bitmap == null)
            {
                _bitmap = new System.Drawing.Bitmap(Application.GetResourceStream(
                         new Uri(GifSource, UriKind.RelativeOrAbsolute)).Stream);
            }

            IntPtr handle = IntPtr.Zero;
            handle = _bitmap.GetHbitmap();

            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(handle);
            return bs;
        }

        private void Initialize()
        {
            //        Console.WriteLine("Init: " + GifSource);
            if (GifSource != null)
                Source = GetSource();
            _isInitialized = true;
        }

        private void FrameUpdatedCallback()
        {
            System.Drawing.ImageAnimator.UpdateFrames();

            if (_source != null)
            {
                _source.Freeze();
            }

            _source = GetSource();

            //  Console.WriteLine("Working: " + GifSource);

            Source = _source;
            InvalidateVisual();
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(FrameUpdatedCallback));
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                this.Initialize();


            //   Console.WriteLine("Start: " + GifSource);

            System.Drawing.ImageAnimator.Animate(_bitmap, OnFrameChanged);
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            _isInitialized = false;
            if (_bitmap != null)
            {
                System.Drawing.ImageAnimator.StopAnimate(_bitmap, OnFrameChanged);
                _bitmap.Dispose();
                _bitmap = null;
            }
            _source = null;
            Initialize();
            GC.Collect();
            GC.WaitForFullGCComplete();

            //   Console.WriteLine("Stop: " + GifSource);
        }

        public void Dispose()
        {
            _isInitialized = false;
            if (_bitmap != null)
            {
                System.Drawing.ImageAnimator.StopAnimate(_bitmap, OnFrameChanged);
                _bitmap.Dispose();
                _bitmap = null;
            }
            _source = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            // Console.WriteLine("Dispose: " + GifSource);
        }
    }
    /// <summary>
    /// Interaction logic for ViewRPC.xaml
    /// </summary>
    public partial class ViewRPCControl : UserControl
    {
        public ViewRPCControl(ViewType view, string Error = "")
        {
            InitializeComponent();
            switch(view)
            {
                case ViewType.Default:
                    {
                        Title.Content = "MultiRPC";
                        Text1.Content = "Thanks for using";
                        Text2.Content = "This program";
                        ViewRPC.Background = SystemColors.ControlDarkDarkBrush;
                        SmallBack.Fill = SystemColors.ControlDarkDarkBrush;

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
                        ViewRPC.Background = SystemColors.ControlDarkDarkBrush;
                        Loading.Visibility = Visibility.Visible;
                        SmallBack.Visibility = Visibility.Hidden;
                    }
                    break;
                case ViewType.Error:
                    {
                        Title.Content = "Error!";
                        Text1.Content = Error;
                        Text2.Content = "";
                        ViewRPC.Background = new SolidColorBrush(new Color { R = 255, G = 57, B = 57, A = 80});
                        LargeImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/ExitIcon.png", UriKind.Absolute));
                        SmallImage.Fill = null;
                        SmallBack.Visibility = Visibility.Hidden;
                    }
                    break;
            }
        }
        public ViewRPCControl(DiscordRPC.Message.PresenceMessage msg)
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
            RPC.Log.ImageError(sender as BitmapImage, e);
        }
    }
}
