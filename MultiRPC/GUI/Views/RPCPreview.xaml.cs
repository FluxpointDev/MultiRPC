using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MultiRPC.Core;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;
using DiscordRPC.Message;
using System.Linq;
using MultiRPC.Core.Rpc;

namespace MultiRPC.GUI.Views
{
    /// <summary>
    /// Interaction logic for RPCPreview.xaml
    /// </summary>
    public partial class RPCPreview : UserControl
    {
        Uri LargeImageUri = null;
        Uri SmallImageUri = null;

        public RPCPreview()
        {
            InitializeComponent();
            UpdateText();
            Settings.Current.LanguageChanged += (_,__) => UpdateText();
        }

        public enum ViewType
        {
            Default,
            Loading,
            Error,
            Preview,
            RichPresence
        }

        private ViewType currentView = ViewType.Default;

        public ViewType CurrentView
        {
            get => currentView;
            set
            {
                currentView = value;
                if (CurrentView == ViewType.RichPresence) 
                {
                    Rpc.RpcUpdated += Rpc_RpcUpdated;
                }
                else
                {
                    Rpc.RpcUpdated -= Rpc_RpcUpdated;
                }

                if (value == ViewType.Preview)
                {
                    UpdateBackground("Purple");
                    UpdateForeground(new SolidColorBrush(Colors.White));
                }
                Dispatcher.Invoke(() => UpdateText());
            }
        }

        private void Rpc_RpcUpdated(object sender, PresenceMessage e) => Dispatcher.Invoke(() => UpdateText(e));

        public void UpdateText(PresenceMessage e = default, CustomProfile customProfile = default, DefaultSettings defaultSettings = default)
        {
            //ToDo: Clean this up
            if (!IsInitialized)
            {
                return;
            }

            if (CurrentView == ViewType.Preview)
            {
                if (customProfile != null) 
                {
                    tblText1.Text = customProfile.Text1;
                    tblText2.Text = customProfile.Text2;

                    UpdateImages(default(Uri), //Just so it knows to use the method we want it to use
                        largeImageText: customProfile.LargeText, 
                        smallImageText: customProfile.SmallText);
                }
                else if (defaultSettings != null)
                {
                    tblText1.Text = defaultSettings.Text1;
                    tblText2.Text = defaultSettings.Text2;

                    UpdateImages(
                        Data.MultiRPCImages.Values.ElementAt(defaultSettings.LargeKey),
                        Data.MultiRPCImages.Values.ElementAt(defaultSettings.SmallKey),
                        largeImageText: defaultSettings.LargeText,
                        smallImageText: defaultSettings.SmallText);
                }
                return;
            }

            var presence = e?.Presence ?? Rpc.Client?.CurrentPresence;

            imgLoading.Visibility = Visibility.Collapsed;
            recLargeImage.Visibility = Visibility.Visible;
            recLargeImage.Margin = new Thickness(0);

            if (CurrentView != ViewType.RichPresence) 
            {
                gridSmallImage.Visibility = Visibility.Collapsed;
            }

            if (CurrentView == ViewType.RichPresence)
            {
                UpdateBackground("Purple");
                UpdateForeground(Brushes.White);
            }
            else if (CurrentView == ViewType.Error)
            {
                UpdateBackground("Red");
            }
            else
            {
                UpdateBackground("AccentColour2SCBrush");
                UpdateForeground("TextColourSCBrush");
            }

            switch (CurrentView)
            {
                case ViewType.Default:
                    {
                        tblTitle.Text = "MultiRPC";

                        tblText1.Text = LanguagePicker.GetLineFromLanguageFile("ThankYouForUsing");
                        tblText2.Text = LanguagePicker.GetLineFromLanguageFile("ThisProgram");

                        tblText1.Visibility = Visibility.Visible;
                        tblText2.Visibility = Visibility.Visible;

                        tblTime.Text = "";
                        var image = App.Current.Resources["MultiRPCLogoDrawingImage"];
                        recLargeImage.Fill =
                            new ImageBrush((ImageSource)image);
                    }
                    break;
                case ViewType.Loading:
                    {
                        tblTitle.Text = LanguagePicker.GetLineFromLanguageFile("Loading") + "...";
                        tblText1.Text = "";
                        tblText2.Text = "";
                        //AnimationBehavior.SetSourceUri(imgLoading,
                        //    new System.Uri("../../Assets/Loading.gif", UriKind.Relative));
                        //AnimationBehavior.SetRepeatBehavior(imgLoading, RepeatBehavior.Forever);
                        imgLoading.Visibility = Visibility.Visible;
                        recLargeImage.Visibility = Visibility.Collapsed;
                    }
                    break;
                case ViewType.Error:
                    {
                        tblTitle.Text = LanguagePicker.GetLineFromLanguageFile("Error") + "!";
                        tblTitle.Foreground = new SolidColorBrush(Colors.White);

                        tblText1.Text = LanguagePicker.GetLineFromLanguageFile("AttemptingToReconnect");
                        tblText1.Foreground = new SolidColorBrush(Colors.White);
                        tblText1.Visibility = Visibility.Visible;

                        tblText2.Text = "";
                        tblText2.Visibility = Visibility.Collapsed;
                        gridBackground.SetResourceReference(Panel.BackgroundProperty, "Red");
                        imgLoading.SetResourceReference(Image.SourceProperty, "DeleteIconDrawingImage");
                        imgLoading.Visibility = Visibility.Visible;

                        recLargeImage.Visibility = Visibility.Collapsed;
                    }
                    break;
                case ViewType.RichPresence:
                    {
                        tblTitle.Text = e?.Name ?? tblTitle.Text;

                        if (presence == null)
                        {
                            break;
                        }

                        tblText1.Text = presence.Details;
                        tblText2.Text = presence.State;
                        UpdateTextVisibility();
                        tblTime.Visibility = Visibility.Visible;

                        UpdateImages(presence.Assets?.LargeImageID, presence.Assets?.SmallImageID, 
                            presence.Assets.LargeImageText, presence.Assets.SmallImageText, ulong.Parse(Rpc.Client.ApplicationID));

                        UpdateTime();
                    }
                    break;
            }
        }

        public void UpdateTextVisibility()
        {
            tblText1.Visibility = string.IsNullOrWhiteSpace(tblText1.Text) ? Visibility.Collapsed : Visibility.Visible;
            tblText2.Visibility = string.IsNullOrWhiteSpace(tblText2.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void UpdateTime()
        {
            async void updateTime() 
            {
                while ((Rpc.RichPresence?.Timestamps?.Start.HasValue ?? false) && Rpc.HasConnection)
                {
                    var ts = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - Rpc.RichPresence.Timestamps.Start.Value.Ticks);
                    tblTime.Text = tblTime.Text = $"{(ts.Hours > 0 ? ts.Hours.ToString().PadLeft(2, '0') + ":" : "")}{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";

                    await Task.Delay(1000);
                }
            }
            updateTime();

            while ((Rpc.RichPresence?.Timestamps?.Start.HasValue ?? false) && Rpc.Client.IsInitialized)
            {
                tblTime.Text = "";

                await Task.Delay(1000);
                updateTime();
            }

            tblTime.Text = "";
        }

        private void UpdateForeground(SolidColorBrush foreground)
        {
            tblTitle.Foreground = foreground;
            tblText1.Foreground = foreground;
            tblText2.Foreground = foreground;
            tblTime.Foreground = foreground;
        }

        private void UpdateForeground(string foreground)
        {
            tblTitle.SetResourceReference(TextBlock.ForegroundProperty, foreground);
            tblText1.SetResourceReference(TextBlock.ForegroundProperty, foreground);
            tblText2.SetResourceReference(TextBlock.ForegroundProperty, foreground);
            tblTime.SetResourceReference(TextBlock.ForegroundProperty, foreground);
        }

        private void UpdateBackground(SolidColorBrush background)
        {
            gridBackground.Background = background;
            ellSmallImageBackground.Fill = background;
        }

        private void UpdateBackground(string background)
        {
            gridBackground.SetResourceReference(Panel.BackgroundProperty, background);
            ellSmallImageBackground.SetResourceReference(Shape.FillProperty, background);
        }

        private void UpdateImages(
            Uri largeImageUri = null,
            Uri smallImageUri = null,
            string largeImageText = "",
            string smallImageText = "")
        {
            if (largeImageUri != null &&
                LargeImageUri != largeImageUri)
            {
                var largeImage = largeImageUri.GetBitmap().ConfigureAwait(false).GetAwaiter();
                largeImage.OnCompleted(() => Dispatcher.Invoke(() =>
                {
                    if (recLargeImage.Fill is ImageBrush imageBrush)
                    {
                        imageBrush.ImageSource = largeImage.GetResult();
                    }
                    else
                    {
                        recLargeImage.Fill = new ImageBrush(largeImage.GetResult());
                    }
                    recLargeImage.Visibility = Visibility.Visible;
                    gridSmallImage.Visibility =
                        smallImageUri == null || ((recLargeImage.Fill as ImageBrush)?.ImageSource ?? null) == null ?
                        Visibility.Collapsed :
                        Visibility.Visible;
                }));
            }
            else if (largeImageUri == null)
            {
                if (recLargeImage.Fill is ImageBrush imageBrush)
                {
                    imageBrush.ImageSource = null;
                }
                else
                {
                    recLargeImage.Fill = new ImageBrush();
                }
                recLargeImage.Visibility = Visibility.Collapsed;
                gridSmallImage.Visibility = Visibility.Collapsed;
            }
            recLargeImage.ToolTip = !string.IsNullOrWhiteSpace(largeImageText) ? new ToolTip(largeImageText) : null;
            LargeImageUri = largeImageUri;

            if (smallImageUri != null &&
                SmallImageUri != smallImageUri)
            {
                var smallImage = smallImageUri.GetBitmap().ConfigureAwait(false).GetAwaiter();
                smallImage.OnCompleted(() => Dispatcher.Invoke(() =>
                {
                    if (ellSmallImage.Fill is ImageBrush smallImageBrush)
                    {
                        smallImageBrush.ImageSource = smallImage.GetResult();
                    }
                    else
                    {
                        ellSmallImage.Fill = new ImageBrush(smallImage.GetResult());
                    }
                }));
            }
            else if (smallImageUri == null)
            {
                if (ellSmallImage.Fill is ImageBrush imageBrush)
                {
                    imageBrush.ImageSource = null;
                }
                else
                {
                    ellSmallImage.Fill = new ImageBrush();
                }
            }

            SmallImageUri = smallImageUri;
            ellSmallImage.ToolTip = !string.IsNullOrWhiteSpace(smallImageText) &&
                ((recLargeImage.Fill as ImageBrush) ?? null) != null ?
                new ToolTip(smallImageText) : null;
        }


        private void UpdateImages(
            ulong? largeImageID = 0, 
            ulong? smallImageID = 0,
            string largeImageText = "", 
            string smallImageText = "", 
            ulong applicationID = Constants.MultiRPCID) =>
            UpdateImages(
                (largeImageID ?? ulong.MinValue) != ulong.MinValue ? new Uri($"https://cdn.discordapp.com/app-assets/{applicationID}/{largeImageID}.png") : null,
                (smallImageID ?? ulong.MinValue) != ulong.MinValue ? new Uri($"https://cdn.discordapp.com/app-assets/{applicationID}/{smallImageID}.png") : null,
                largeImageText, 
                smallImageText);
    }
}