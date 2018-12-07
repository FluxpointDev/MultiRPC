using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MultiRPC.GUI
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
}
