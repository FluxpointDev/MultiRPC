using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DiscordRPC.Message;
using MultiRPC.Functions;
using XamlAnimatedGif;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;
using System.Extra;

namespace MultiRPC.GUI.Views
{
    /// <summary>
    ///     Interaction logic for RPCPreview.xaml
    /// </summary>
    public partial class RPCPreview : UserControl
    {
        public enum ViewType
        {
            Default,
            Default2,
            Loading,
            Error,
            Blank,
            RichPresence
        }

        public ViewType CurrentViewType;

        public RPCPreview(ViewType view, string error = "", SolidColorBrush background = null,
            SolidColorBrush foreground = null, string backgroundName = null, string foregroundName = null)
        {
            InitializeComponent();
            UpdateUIViewType(view, error, background, foreground, backgroundName, foregroundName);
        }

        public RPCPreview(PresenceMessage msg)
        {
            InitializeComponent();

            CurrentViewType = ViewType.RichPresence;
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
                    var largeImage = (new [] {
                        "https://cdn.discordapp.com/app-assets",
                        msg.ApplicationID,
                        msg.Presence.Assets.LargeImageID + ".png" }.CombineToUri()).DownloadImage()
                        .ConfigureAwait(false).GetAwaiter().GetResult();

                    recLargeImage.Fill = new ImageBrush(largeImage);
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.LargeImageText))
                        recLargeImage.ToolTip = new ToolTip(msg.Presence.Assets.LargeImageText);
                }
                else
                {
                    recLargeImage.Fill = null;
                }

                if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageKey) &&
                    recLargeImage.Visibility == Visibility.Visible)
                {
                    var smallImage = new [] {
                            "https://cdn.discordapp.com/app-assets",
                            msg.ApplicationID,
                            msg.Presence.Assets.SmallImageID + ".png"}
                            .CombineToUri()
                            .DownloadImage()
                        .ConfigureAwait(false)
                        .GetAwaiter().GetResult();
                    ellSmallImage.Fill = new ImageBrush(smallImage);
                    if (!string.IsNullOrEmpty(msg.Presence.Assets.SmallImageText))
                        ellSmallImage.ToolTip = new ToolTip(msg.Presence.Assets.SmallImageText);
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

        private Task UpdateBackground(SolidColorBrush background)
        {
            gridBackground.Background = background;
            ellSmallImageBackground.Fill = background;

            return Task.CompletedTask;
        }

        private Task UpdateForeground(SolidColorBrush foreground)
        {
            tblTitle.Foreground = foreground;
            tblText1.Foreground = foreground;
            tblText2.Foreground = foreground;
            tblTime.Foreground = foreground;

            return Task.CompletedTask;
        }

        private Task UpdateBackground(string background)
        {
            gridBackground.SetResourceReference(Panel.BackgroundProperty, background);
            ellSmallImageBackground.SetResourceReference(Shape.FillProperty, background);

            return Task.CompletedTask;
        }

        private Task UpdateForeground(string foreground)
        {
            tblTitle.SetResourceReference(TextBlock.ForegroundProperty, foreground);
            tblText1.SetResourceReference(TextBlock.ForegroundProperty, foreground);
            tblText2.SetResourceReference(TextBlock.ForegroundProperty, foreground);
            tblTime.SetResourceReference(TextBlock.ForegroundProperty, foreground);

            return Task.CompletedTask;
        }

        public Task UpdateTime(TimeSpan ts)
        {
            if (CurrentViewType == ViewType.Error)
            {
                tblTime.Text = "";
                return Task.CompletedTask;
            }

            tblTime.Text = ts.Hours == 0
                ? $"{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}"
                : $"{ts.Hours.ToString().PadLeft(2, '0')}:{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";

            return Task.CompletedTask;
        }

        public async Task UpdateUIViewType(ViewType view, string error = "", SolidColorBrush background = null,
            SolidColorBrush foreground = null, string backgroundName = null, string foregroundName = null)
        {
            ellSmallImage.ToolTip = null;
            recLargeImage.ToolTip = null;

            CurrentViewType = view;
            if (background == null && string.IsNullOrWhiteSpace(backgroundName))
                backgroundName = "AccentColour2SCBrush";
            if (!string.IsNullOrWhiteSpace(backgroundName))
                UpdateBackground(backgroundName);
            else
                UpdateBackground(background);

            if (foreground == null && string.IsNullOrWhiteSpace(foregroundName))
                foregroundName = "TextColourSCBrush";
            if (!string.IsNullOrWhiteSpace(foregroundName))
                UpdateForeground(foregroundName);
            else
                UpdateForeground(foreground);

            imgLoading.Visibility = Visibility.Collapsed;
            recLargeImage.Visibility = Visibility.Visible;
            gridSmallImage.Visibility = Visibility.Collapsed;
            recLargeImage.Margin = new Thickness(0);
            switch (view)
            {
                case ViewType.Blank:
                {
                    tblTitle.Text = "MultiRPC";
                    tblText1.Visibility = Visibility.Visible;
                    tblText2.Visibility = Visibility.Visible;
                    tblTime.Text = "";
                    recLargeImage.Fill =
                        new ImageBrush((ImageSource) Application.Current.Resources["MultiRPCLogoDrawingImage"]);
                }
                    break;
                case ViewType.Default:
                {
                    tblTitle.Text = "MultiRPC";
                    tblText1.Text = App.Text.ThankYouForUsing;
                    tblText1.Visibility = Visibility.Visible;
                    tblText2.Text = App.Text.ThisProgram;
                    tblText2.Visibility = Visibility.Visible;
                    tblTime.Text = "";
                    recLargeImage.Fill =
                        new ImageBrush((ImageSource) Application.Current.Resources["MultiRPCLogoDrawingImage"]);
                }
                    break;
                case ViewType.Default2:
                {
                    tblTitle.Text = "MultiRPC";
                    tblText1.Text = App.Text.Hello;
                    tblText1.Visibility = Visibility.Visible;
                    tblText2.Text = App.Text.World;
                    tblText2.Visibility = Visibility.Visible;
                    recLargeImage.Fill =
                        new ImageBrush((ImageSource) Application.Current.Resources["MultiRPCLogoDrawingImage"]);
                }
                    break;
                case ViewType.Loading:
                {
                    tblTitle.Text = $"{App.Text.Loading}...";
                    tblText1.Text = "";
                    tblText2.Text = "";
                    AnimationBehavior.SetSourceUri(imgLoading,
                        new System.Uri("../../Assets/Loading.gif", UriKind.Relative));
                    AnimationBehavior.SetRepeatBehavior(imgLoading, RepeatBehavior.Forever);
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
                    gridBackground.SetResourceReference(Panel.BackgroundProperty, "Red");
                    imgLoading.Source = (DrawingImage) Application.Current.Resources["DeleteIconDrawingImage"];
                    imgLoading.Visibility = Visibility.Visible;
                    recLargeImage.Visibility = Visibility.Collapsed;
                }
                    break;
            }
        }

        private Visibility TextShouldBeVisible(TextBlock textBlock)
        {
            return !string.IsNullOrWhiteSpace(textBlock.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Task UpdateTextVisibility()
        {
            tblText1.Visibility = TextShouldBeVisible(tblText1);
            tblText2.Visibility = TextShouldBeVisible(tblText2);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Update the Image
        /// </summary>
        /// <param name="onSmallImage">If to target the small image</param>
        /// <param name="image">Image</param>
        public void UpdateImage(bool onSmallImage, ImageBrush image)
        {
            if (!onSmallImage && image?.ImageSource == null)
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

            if (onSmallImage && image?.ImageSource == null)
            {
                ellSmallImage.Fill = null;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
            else if (onSmallImage)
            {
                ellSmallImage.Fill = image;
                if (recLargeImage.Visibility != Visibility.Collapsed)
                    gridSmallImage.Visibility = Visibility.Visible;
            }
        }
    }
}