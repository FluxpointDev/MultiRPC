using System;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using DiscordRPC.Message;

namespace MultiRPC.GUI.Views
{
    /// <summary>
    /// Interaction logic for RPCPreview.xaml
    /// </summary>
    public partial class RPCPreview : UserControl
    {
        public ViewType CurrentViewType;

        public enum ViewType
        {
            Default, Default2, Loading, Error
        }

        public async Task UpdateBackground(SolidColorBrush background)
        {
            gridBackground.Background = background;
            ellSmallImageBackground.Fill = background;
        }
        public async Task UpdateForground(SolidColorBrush forground)
        {
            tblTitle.Foreground = forground;
            tblText1.Foreground = forground;
            tblText2.Foreground = forground;
            tblTime.Foreground = forground;
        }

        public RPCPreview(ViewType view, string error = "", SolidColorBrush background = null, SolidColorBrush forground = null)
        {
            InitializeComponent();
            UpdateUIViewType(view, error, background, forground);
        }

        public async Task UpdateTime(TimeSpan ts)
        {
            if (CurrentViewType == ViewType.Error)
            {
                tblTime.Text = "";
                return;
            }

            if (ts.Hours == 0)
                tblTime.Text = $"{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";
            else
                tblTime.Text = $"{ts.Hours.ToString().PadLeft(2, '0')}:{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";
        }

        public async Task UpdateUIViewType(ViewType view, string error = "", SolidColorBrush background = null, SolidColorBrush forground = null)
        {
            CurrentViewType = view;
            if (background == null)
                background = (SolidColorBrush)Application.Current.Resources["AccentColour2SCBrush"];
            UpdateBackground(background);

            if (forground == null)
                forground = (SolidColorBrush)Application.Current.Resources["TextColourSCBrush"];
            UpdateForground(forground);

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
                        gridBackground.Background = (SolidColorBrush)Application.Current.Resources["Red"];
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
            
            UpdateBackground((SolidColorBrush)App.Current.Resources["Purple"]);
            UpdateForground(Brushes.White);
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
                    BitmapImage Large = new BitmapImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.LargeImageID + ".png"));
                    Large.DownloadFailed += Image_FailedLoading;
                    recLargeImage.Fill = new ImageBrush(Large);
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageText))
                        recLargeImage.ToolTip = new Controls.ToolTip(msg.Presence.Assets.LargeImageText);
                }
                else
                    recLargeImage.Fill = null;
                if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageKey) && recLargeImage.Visibility == Visibility.Visible)
                {
                    BitmapImage Small = new BitmapImage(new Uri("https://cdn.discordapp.com/app-assets/" + msg.ApplicationID + "/" + msg.Presence.Assets.SmallImageID + ".png"));
                    Small.DownloadFailed += Image_FailedLoading;
                    ellSmallImage.Fill = new ImageBrush(Small);
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

        public Visibility TextShouldBeVisibile(TextBlock textBlock)
        {
            if (!string.IsNullOrWhiteSpace(textBlock.Text))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public async Task UpdateTextVisibility()
        {
            tblText1.Visibility = TextShouldBeVisibile(tblText1);
            tblText2.Visibility = TextShouldBeVisibile(tblText2);
        }

        /// <summary>
        /// Update the Image
        /// </summary>
        /// <param name="OnSmallImage">If to target the small image</param>
        /// <param name="Image">Image for Small Image</param>
        /// <param name="Source">Image for Large Image</param>
        public void UpdateImage(bool OnSmallImage, ImageBrush Image)
        {
            if (!OnSmallImage && (Image == null || Image.ImageSource == null))
            {
                recLargeImage.Fill = null;
                recLargeImage.Visibility = Visibility.Collapsed;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
            else if (!OnSmallImage)
            {
                recLargeImage.Fill = Image;
                recLargeImage.Visibility = Visibility.Visible;
                if (gridSmallImage.Visibility == Visibility.Collapsed && ellSmallImage.Fill != null)
                    gridSmallImage.Visibility = Visibility.Visible;
            }

            if (OnSmallImage && (Image == null || Image.ImageSource == null))
            {
                ellSmallImage.Fill = null;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
            else if (OnSmallImage)
            {
                ellSmallImage.Fill = Image;
                if(recLargeImage.Visibility != Visibility.Collapsed)
                    gridSmallImage.Visibility = Visibility.Visible;
            }
        }

        public static void Image_FailedLoading(object sender, ExceptionEventArgs e)
        {
            App.Logging.ImageError(sender as BitmapImage, e);
        }
    }
}
