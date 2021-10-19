using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg;
using DiscordRPC;
using MultiRPC.Extensions;

namespace MultiRPC.UI.Views
{
    public enum ViewType
    {
        Default,
        Default2,
        Loading,
        Error,
        RichPresence
    }
    
    public partial class RpcView : UserControl
    {
        public RpcView()
        {
            InitializeComponent();
            
            //TODO: See if SVG has some kind of aliasing because it's a bit too sharp on this...
            brdLarge.Background = 
                _logoVisualBrush = new VisualBrush(new Image {
                        Source = new SvgImage
                        {
                            Source = SvgSource.Load("avares://MultiRPC/Assets/Logo.svg", null)
                        }
                    });
            
            tblTitle.DataContext = _titleText;
            tblText1.DataContext = _tblText1;
            tblText2.DataContext = _tblText2;
            ViewType = ViewType.Default;
        }

        private ViewType _viewType;
        public ViewType ViewType
        {
            get => _viewType;
            set
            {
                _viewType = value;
                this.RunUILogic(() => UpdateFromType());
            }
        }

        private void UpdateFromRichPresence(RichPresence? presence)
        {
            //TODO: Make
        }

        private readonly Language _titleText = new Language();
        private readonly Language _tblText1 = new Language();
        private readonly Language _tblText2 = new Language();
        private readonly VisualBrush _logoVisualBrush;
        
        private void UpdateFromType(string? error = null, RichPresence? richPresence = null)
        {
            tblText1.IsVisible = _viewType is not ViewType.Loading or ViewType.RichPresence;
            tblText2.IsVisible = tblText1.IsVisible && _viewType is not ViewType.Error;
            tblTime.IsVisible = _viewType == ViewType.RichPresence;
            gridSmallImage.IsVisible = tblTime.IsVisible;

            var brush = _viewType switch
            {
                ViewType.Default => Application.Current.Resources["ThemeAccentBrush2"],
                ViewType.Default2 => Application.Current.Resources["ThemeAccentBrush2"],
                ViewType.Loading => Application.Current.Resources["PurpleBrush"],
                ViewType.Error => Application.Current.Resources["RedBrush"],
                ViewType.RichPresence => Application.Current.Resources["PurpleBrush"],
                _ => Application.Current.Resources["ThemeAccentBrush2"]
            };
            brdContent.Background = (IBrush)brush!;

            switch (_viewType)
            {
                case ViewType.Default:
                {
                    _titleText.ChangeJsonNames("MultiRPC");
                    _tblText1.ChangeJsonNames("ThankYouForUsing");
                    _tblText2.ChangeJsonNames("ThisProgram");
                    brdLarge.Background = _logoVisualBrush;
                }
                break;
                case ViewType.Default2:
                {
                    _titleText.ChangeJsonNames("MultiRPC");
                    _tblText1.ChangeJsonNames("Hello");
                    _tblText2.ChangeJsonNames("World");
                    brdLarge.Background = _logoVisualBrush;
                }
                break;
                case ViewType.Loading:
                {
                    //TODO: Add the ... and load animation
                    _titleText.ChangeJsonNames("Loading");
                    /*AnimationBehavior.SetSourceUri(imgLoading,
                        new System.Uri("../../Assets/Loading.gif", UriKind.Relative));
                    AnimationBehavior.SetRepeatBehavior(imgLoading, RepeatBehavior.Forever);
                    imgLoading.Visibility = Visibility.Visible;
                    recLargeImage.Visibility = Visibility.Collapsed;*/
                }
                break;
                case ViewType.Error:
                {
                    _titleText.ChangeJsonNames("Error");

                    tblTitle.Foreground = new SolidColorBrush(Colors.White);
                    tblText1.Text = error;
                    tblText1.Foreground = new SolidColorBrush(Colors.White);
                    /*imgLoading.Source = (DrawingImage) Application.Current.Resources["DeleteIconDrawingImage"];
                    imgLoading.Visibility = Visibility.Visible;
                    recLargeImage.Visibility = Visibility.Collapsed;*/
                }
                break;
                case ViewType.RichPresence:
                {
                    UpdateFromRichPresence(richPresence);
                }
                break;
            }
        }
    }
}