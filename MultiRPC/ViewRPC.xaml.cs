using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace MultiRPC
{
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
    public partial class ViewRPC : UserControl
    {
        public ViewRPC(string title, string text1, string text2, string largeImage, string smallImage, string largeText, string smallText)
        {
            InitializeComponent();
            Loading.Visibility = Visibility.Hidden;
            if (title == "loadthispls")
            {
                Title.Content = "Loading RPC";
                Text1.Content = "";
                Text2.Content = "";
                Background.Background = SystemColors.ActiveBorderBrush;
                Loading.Visibility = Visibility.Visible;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(Directory.GetCurrentDirectory() + "/Loading.gif", UriKind.Absolute);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(Loading, image);
                ImageBehavior.SetRepeatBehavior(Loading, System.Windows.Media.Animation.RepeatBehavior.Forever);
                SmallBackground.Visibility = Visibility.Hidden;
            }
            else
            {
                Title.Content = title;
                Text1.Content = text1;
                Text2.Content = text2;
                if (!string.IsNullOrEmpty(largeImage))
                {
                    ImageSource LargeSource = new BitmapImage(new Uri(largeImage));
                    LargeImage.Source = LargeSource;
                    LargeImage.ToolTip = new Button().Content = largeText;
                }
                if (!string.IsNullOrEmpty(smallImage))
                {
                    ImageSource SmallSource = new BitmapImage(new Uri(smallImage));
                    SmallImage.Source = SmallSource;
                    SmallImage.ToolTip = new Button().Content = smallText;
                }
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
                    SmallImage.Source = new BitmapImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.SmallImageID + ".png"));
                    SmallImage.ToolTip = new Button().Content = msg.Presence.Assets.SmallImageText;
                }
                else
                {
                    SmallBackground.Visibility = Visibility.Hidden;
                    SmallImage.Visibility = Visibility.Hidden;
                }
                if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageKey))
                {
                    LargeImage.Source = new BitmapImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.LargeImageID + ".png"));
                    LargeImage.ToolTip = new Button().Content = msg.Presence.Assets.LargeImageText;
                }
                else
                    LargeImage.Visibility = Visibility.Hidden;
            }
            
        }
    }
}
