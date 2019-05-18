using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MultiRPC.Functions
{
    public static class BitmapDownloader
    {
        public static async Task<BitmapImage> DownloadImage(Uri uri)
        {
            var b = new BitmapImage();

            b.BeginInit();
            b.UriSource = uri;
            b.EndInit();
            b.DownloadFailed += Image_FailedLoading;
            return b;
        }

        private static void Image_FailedLoading(object sender, ExceptionEventArgs e)
        {
            App.Logging.ImageError(sender as BitmapImage, e);
        }
    }
}