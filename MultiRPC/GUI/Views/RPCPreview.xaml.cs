using System;
using System.Windows;
using MultiRPC.Functions;
using DiscordRPC.Message;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MultiRPC.GUI.Views
{
    /// <summary>
    /// Interaction logic for RPCPreview.xaml
    /// </summary>
    public partial class RPCPreview : UserControl
    {
        private ViewType CurrentViewType;

        public enum ViewType
        {
            Default, Default2, Loading, Error
        }

        public Task UpdateBackground(SolidColorBrush background)
        {
            gridBackground.Background = background;
            ellSmallImageBackground.Fill = background;
            return Task.CompletedTask;
        }
        public Task UpdateForeground(SolidColorBrush forground)
        {
            tblTitle.Foreground = forground;
            tblText1.Foreground = forground;
            tblText2.Foreground = forground;
            tblTime.Foreground = forground;
            return Task.CompletedTask;
        }

        public Task UpdateBackground(string background)
        {
            gridBackground.SetResourceReference(Grid.BackgroundProperty, background);
            ellSmallImageBackground.SetResourceReference(Ellipse.FillProperty, background);
            return Task.CompletedTask;
        }
        public Task UpdateForeground(string forground)
        {
            tblTitle.SetResourceReference(TextBlock.ForegroundProperty, forground);
            tblText1.SetResourceReference(TextBlock.ForegroundProperty, forground);
            tblText2.SetResourceReference(TextBlock.ForegroundProperty, forground);
            tblTime.SetResourceReference(TextBlock.ForegroundProperty, forground);
            return Task.CompletedTask;
        }

        public RPCPreview(ViewType view, string error = "", SolidColorBrush background = null, SolidColorBrush foreground = null, string backgroundName = null, string foregroundName = null)
        {
            InitializeComponent();
            UpdateUIViewType(view, error, background, foreground, backgroundName, foregroundName);
        }

        public Task UpdateTime(TimeSpan ts)
        {
            if (CurrentViewType == ViewType.Error)
            {
                tblTime.Text = "";
                return Task.CompletedTask;
            }

            tblTime.Text = ts.Hours == 0 ? 
                $"{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}" : 
                $"{ts.Hours.ToString().PadLeft(2, '0')}:{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";
            return Task.CompletedTask;
        }

        public async Task UpdateUIViewType(ViewType view, string error = "", SolidColorBrush background = null, SolidColorBrush foreground = null, string backgroundName = null, string foregroundName = null)
        {
            CurrentViewType = view;
            if (background == null && string.IsNullOrWhiteSpace(backgroundName))
                backgroundName = "AccentColour2SCBrush";
            if (!string.IsNullOrWhiteSpace(backgroundName))
            {
                UpdateBackground(backgroundName);
            }
            else
            {
                UpdateBackground(background);
            }

            if (foreground == null && string.IsNullOrWhiteSpace(foregroundName))
                foregroundName = "TextColourSCBrush";
            if (!string.IsNullOrWhiteSpace(foregroundName))
            {
                UpdateForeground(foregroundName);
            }
            else
            {
                UpdateForeground(foreground);
            }

            imgLoading.Visibility = Visibility.Collapsed;
            recLargeImage.Visibility = Visibility.Visible;
            gridSmallImage.Visibility = Visibility.Collapsed;
            recLargeImage.Margin = new Thickness(0);
            switch (view)
            {
                case ViewType.Default:
                    {
                        tblTitle.Text = "MultiRPC";
                        tblText1.Text = App.Text.ThankYouForUsing;
                        tblText1.Visibility = Visibility.Visible;
                        tblText2.Text = App.Text.ThisProgram;
                        tblText2.Visibility = Visibility.Visible;
                        tblTime.Text = "";
                        recLargeImage.Fill = new ImageBrush((ImageSource)App.Current.Resources["MultiRPCLogoDrawingImage"]);
                    }
                    break;
                case ViewType.Default2:
                    {
                        tblTitle.Text = "MultiRPC";
                        tblText1.Text = App.Text.Hello;
                        tblText1.Visibility = Visibility.Visible;
                        tblText2.Text = App.Text.World;
                        tblText2.Visibility = Visibility.Visible;
                        recLargeImage.Fill = new ImageBrush((ImageSource)App.Current.Resources["MultiRPCLogoDrawingImage"]);
                    }
                    break;
                case ViewType.Loading:
                    {
                        tblTitle.Text = $"{App.Text.Loading}...";
                        tblText1.Text = "";
                        tblText2.Text = "";
                        XamlAnimatedGif.AnimationBehavior.SetSourceUri(imgLoading, new Uri("../../Assets/Loading.gif", UriKind.Relative));
                        XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(imgLoading, RepeatBehavior.Forever);
                        imgLoading.Visibility = Visibility.Visible;
                        recLargeImage.Visibility = Visibility.Collapsed;
                    }
                    break;
                case ViewType.Error:
                    {
                        tblTitle.Text = $"{App.Text.Error}!";
                        tblTitle.Foreground = new SolidColorBrush(Colors.White);
                        tblText1.Text = error;
                        tblText1.Foreground = new SolidColorBrush(Colors.White);
                        gridBackground.SetResourceReference(Grid.BackgroundProperty, "Red");
                        imgLoading.Source = (DrawingImage)App.Current.Resources["DeleteIconDrawingImage"];
                        imgLoading.Visibility = Visibility.Visible;
                        recLargeImage.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        public RPCPreview(PresenceMessage msg)
        {
            InitializeComponent();
            
            UpdateBackground("Purple");
            UpdateForeground(Brushes.White);
            tblTitle.Text = msg.Name;
            tblText1.Text = msg.Presence.Details;
            tblText2.Text = msg.Presence.State;
            UpdateTextVisibility();
            tblTime.Visibility = Visibility.Visible;

            if (msg.Presence.HasAssets())
            {
                recLargeImage.Visibility = Visibility.Collapsed;
                if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageKey))
                {
                    recLargeImage.Visibility = Visibility.Visible;
                    BitmapImage largeImage = BitmapDownloader.DownloadImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.LargeImageID + ".png")).ConfigureAwait(false).GetAwaiter().GetResult();
                    recLargeImage.Fill = new ImageBrush(largeImage);
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageText))
                        recLargeImage.ToolTip = new Controls.ToolTip(msg.Presence.Assets.LargeImageText);
                }
                else
                    recLargeImage.Fill = null;
                if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageKey) && recLargeImage.Visibility == Visibility.Visible)
                {
                    BitmapImage smallImage = BitmapDownloader.DownloadImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.SmallImageID + ".png")).ConfigureAwait(false).GetAwaiter().GetResult();
                    ellSmallImage.Fill = new ImageBrush(smallImage);
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageText))
                        ellSmallImage.ToolTip = new Controls.ToolTip(msg.Presence.Assets.SmallImageText);
                }
                else
                {
                    gridSmallImage.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                recLargeImage.Visibility = Visibility.Collapsed;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
        }

        public Visibility TextShouldBeVisible(TextBlock textBlock)
        {
            if (!string.IsNullOrWhiteSpace(textBlock.Text))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public async Task UpdateTextVisibility()
        {
            tblText1.Visibility = TextShouldBeVisible(tblText1);
            tblText2.Visibility = TextShouldBeVisible(tblText2);
        }

        /// <summary>
        /// Update the Image
        /// </summary>
        /// <param name="OnSmallImage">If to target the small image</param>
        /// <param name="Image">Image for Small Image</param>
        /// <param name="Source">Image for Large Image</param>
        public void UpdateImage(bool onSmallImage, ImageBrush image)
        {
            if (!onSmallImage && (image == null || image.ImageSource == null))
            {
                recLargeImage.Fill = null;
                recLargeImage.Visibility = Visibility.Collapsed;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
            else if (!onSmallImage)
            {
                recLargeImage.Fill = image;
                recLargeImage.Visibility = Visibility.Visible;
                if (gridSmallImage.Visibility == Visibility.Collapsed && ellSmallImage.Fill != null)
                    gridSmallImage.Visibility = Visibility.Visible;
            }

            if (onSmallImage && (image == null || image.ImageSource == null))
            {
                ellSmallImage.Fill = null;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
            else if (onSmallImage)
            {
                ellSmallImage.Fill = image;
                if(recLargeImage.Visibility != Visibility.Collapsed)
                    gridSmallImage.Visibility = Visibility.Visible;
            }
        }
    }
}
