using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MultiRPC.Core;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Notification;

namespace MultiRPC
{
    /// <summary>
    /// Helper for downloading bitmaps
    /// </summary>
    public static class BitmapDownloader
    {
        private static HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets the bitmap from the uri
        /// </summary>
        /// <param name="uri">Location of the image</param>
        public async static Task<BitmapImage> GetBitmap(this Uri uri) 
        {
            if (!Utils.NetworkIsAvailable() || uri == null)
            {
                return null;
            }

            try
            {
                var stream = await httpClient.GetStreamAsync(uri.AbsoluteUri);
                var image = new BitmapImage();
                image.BeginInit();
                image.SetCurrentValue(BitmapImage.StreamSourceProperty, stream);
                image.EndInit();

                return image;
            }
            catch (Exception e)
            {
                NotificationCenter.Logger.Error(e);
                return null;
            }
        }
    }
}
