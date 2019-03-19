using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MultiRPC.Functions
{
    public static class BitmapDownloader
    {
        public async static Task<BitmapImage> DownloadImage(Uri uri)
        {
            BitmapImage b = new BitmapImage();

            b.BeginInit();
            b.UriSource = uri;
            b.EndInit();
            return b;
        }
    }
}
