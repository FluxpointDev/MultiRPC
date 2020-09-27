using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static MultiRPC.Core.LanguagePicker;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RPCView : LocalizablePage, INotifyPropertyChanged
    {
        //TODO: Make this page not take up so much dam ram (cachingTM)
        readonly ToolTip largeIconTooltip = new ToolTip();
        readonly ToolTip smallIconTooltip = new ToolTip();

        public RPCView()
        {
            InitializeComponent();
        }

        public enum ViewType
        {
            Default,
            Default2,
            Loading,
            Error,
            RichPresence
        }

        private ViewType currentView = ViewType.Default;
        public ViewType CurrentView 
        {
            get => currentView;
            set
            {
                currentView = value;

                if (IsLoaded)
                {
                    UpdateText();
                }
            }
        }

        private RichPresence richPresence;
        public RichPresence RichPresence
        {
            get => richPresence;
            set
            {
                if (richPresence != null)
                {
                    richPresence.PropertyChanged -= RichPresence_PropertyChanged;
                }
                richPresence = value;
                richPresence.PropertyChanged += RichPresence_PropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //TODO: Hook up
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
            UpdateText();

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value ?? RichPresence?.ApplicationName ?? "";
                OnPropertyChanged();
            }
        }

        private string text1;
        public string Text1
        {
            get => text1;
            set
            {
                text1 = value ?? RichPresence?.State ?? "";
                OnPropertyChanged();
            }
        }

        private string text2;
        public string Text2
        {
            get => text2;
            set
            {
                text2 = value ?? RichPresence?.Details;
                OnPropertyChanged();
            }
        }

        private Brush largeImage;
        public Brush LargeImage
        {
            get => largeImage;
            set
            {
                largeImage = value ?? new ImageBrush
                {
                    ImageSource = new BitmapImage()
                    {
                        UriSource = RichPresence?.Assets?.LargeImage?.Uri
                    }
                };
                OnPropertyChanged();
            }
        }

        private Brush smallImage;
        public Brush SmallImage
        {
            get => smallImage;
            set
            {
                smallImage = value ??
                new ImageBrush
                {
                    ImageSource = new BitmapImage()
                    {
                        UriSource = RichPresence?.Assets?.SmallImage?.Uri
                    }
                };
                OnPropertyChanged();
            }
        }

        public async override void UpdateText()
        {
            var largeIconText = "";
            var smallIconText = "";

            Brush forgroundColour = (Brush)Application.Current.Resources["TextColour2"];
            Brush bgColour = (Brush)Application.Current.Resources["Colour2"];

            switch (CurrentView)
            {
                case ViewType.Default:
                    {
                        Title = GetLineFromLanguageFile("MultiRPC");
                        Text1 = GetLineFromLanguageFile("ThankYouForUsing");
                        Text2 = GetLineFromLanguageFile("ThisProgram");

                        LargeImage = new ImageBrush() 
                        {
                            ImageSource = await AssetManager.GetAsset("Icon/Logo") as ImageSource
                        };

                        UpdateVisibility(regLarge, true);
                        UpdateVisibility(grdSmall, false);
                    }
                    break;
                case ViewType.Default2:
                    {
                        Title = GetLineFromLanguageFile("MultiRPC");
                        Text1 = GetLineFromLanguageFile("Hello");
                        Text2 = GetLineFromLanguageFile("World");

                        LargeImage = new ImageBrush()
                        {
                            ImageSource = await AssetManager.GetAsset("Icon/Logo") as ImageSource
                        };

                        UpdateVisibility(regLarge, true);
                        UpdateVisibility(grdSmall, false);
                    }
                    break;
                case ViewType.Loading:
                    {
                        Title = GetLineFromLanguageFile("Loading") + "...";

                        bgColour = (Brush)Application.Current.Resources["Purple"];
                        forgroundColour = new SolidColorBrush(Colors.White);

                        LargeImage = new ImageBrush()
                        {
                            ImageSource = await AssetManager.GetAsset("Icon/Gif/Loading") as ImageSource
                        };

                        UpdateVisibility(regLarge, true);
                        UpdateVisibility(grdSmall, false);
                    }
                    break;
                case ViewType.Error:
                    {
                        Title = GetLineFromLanguageFile("Error") + "!";
                        Text1 = GetLineFromLanguageFile("AttemptingToReconnect");

                        LargeImage = new ImageBrush()
                        {
                            ImageSource = await AssetManager.GetAsset("Icon/Delete") as ImageSource
                        };

                        bgColour = (Brush)Application.Current.Resources["Red"];
                        forgroundColour = new SolidColorBrush(Colors.White);

                        UpdateVisibility(regLarge, true);
                        UpdateVisibility(grdSmall, false);
                    }
                    break;
                case ViewType.RichPresence:
                    {
                        var smallImageDownloaded = false;
                        var largeImageDownloaded = false;

                        SmallImage = null;
                        LargeImage = null;

                        if ((SmallImage as ImageBrush)?.ImageSource is BitmapImage smallbitmapImage)
                        {
                            smallbitmapImage.ImageOpened += (object sender, RoutedEventArgs e) => 
                            {
                                if (smallbitmapImage != (SmallImage as ImageBrush)?.ImageSource as BitmapImage)
                                {
                                    return;
                                }

                                smallImageDownloaded = true;
                                if (largeImageDownloaded)
                                {
                                    UpdateVisibility(regLarge, true);
                                    UpdateVisibility(grdSmall, true);
                                }
                            };
                        }

                        if ((LargeImage as ImageBrush)?.ImageSource is BitmapImage largebitmapImage)
                        {
                            largebitmapImage.ImageOpened += (object sender, RoutedEventArgs e) =>
                            {
                                if (largebitmapImage != (LargeImage as ImageBrush)?.ImageSource as BitmapImage)
                                {
                                    return;
                                }

                                largeImageDownloaded = true;
                                if (smallImageDownloaded)
                                {
                                    UpdateVisibility(grdSmall, true);
                                }
                                UpdateVisibility(regLarge, true);
                            };
                        }

                        UpdateVisibility(grdSmall, false);
                        UpdateVisibility(regLarge, false);

                        smallIconText = RichPresence?.Assets?.SmallImage?.Text;
                        largeIconText = RichPresence?.Assets?.LargeImage?.Text;

                        //TODO: Add timer logic
                        bgColour = (Brush)Application.Current.Resources["Purple"];
                        forgroundColour = new SolidColorBrush(Colors.White);

                        Title = null;
                        Text1 = null;
                        Text2 = null;
                    }
                    break;
            }
            bdrContainer.Background = bgColour;

            tblTitle.Foreground = forgroundColour;

            tblLine1.Foreground = forgroundColour;
            UpdateVisibility(tblLine1, !string.IsNullOrEmpty(text1));

            tblLine2.Foreground = forgroundColour;
            UpdateVisibility(tblLine2, !string.IsNullOrEmpty(text2));

            smallIconTooltip.Content = smallIconText;
            largeIconTooltip.Content = largeIconText;
            ToolTipService.SetToolTip(regLarge, !string.IsNullOrEmpty(largeIconText) ? largeIconTooltip : null);
            ToolTipService.SetToolTip(elpSmall, !string.IsNullOrEmpty(smallIconText) ? smallIconTooltip : null);

            elpSmallBG.Fill = bgColour;
            UpdateLayout();
        }

        public void ShouldBeVisible(Action task) => task();

        public void UpdateVisibility(UIElement ui, bool shouldBeVisible)
        {
            if (ui != null)
            {
                ui.Visibility = shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
