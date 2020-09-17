using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using static MultiRPC.Core.LanguagePicker;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RPCView : LocalizablePage
    {
        //TODO: Make
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
                UpdateText();
            }
        }


        public async override void UpdateText()
        {
            var title = "";
            var text1 = "";
            var text2 = "";
            object largeIcon = null;
            object smallIcon = null;
            Brush bgColour = (Brush)Application.Current.Resources["Colour2"];

            switch (CurrentView)
            {
                case ViewType.Default:
                    {
                        title = GetLineFromLanguageFile("MultiRPC");
                        text1 = GetLineFromLanguageFile("ThankYouForUsing");
                        text2 = GetLineFromLanguageFile("ThisProgram");

                        largeIcon = await AssetManager.GetAsset("Icon/Logo");
                    }
                    break;
                case ViewType.Default2:
                    {
                        title = GetLineFromLanguageFile("MultiRPC");
                        text1 = GetLineFromLanguageFile("Hello");
                        text2 = GetLineFromLanguageFile("World");

                        largeIcon = await AssetManager.GetAsset("Icon/Logo");
                    }
                    break;
                case ViewType.Loading:
                    {
                        title = GetLineFromLanguageFile("Loading") + "...";

                        bgColour = (SolidColorBrush)Application.Current.Resources["Purple"];
                        largeIcon = await AssetManager.GetAsset("Icon/Gif/Loading");
                    }
                    break;
                case ViewType.Error:
                    {
                        title = GetLineFromLanguageFile("Error") + "!";
                        tblTitle.Foreground = new SolidColorBrush(Colors.White);

                        text1 = GetLineFromLanguageFile("AttemptingToReconnect");
                        tblLine1.Foreground = new SolidColorBrush(Colors.White);

                        bgColour = (SolidColorBrush)Application.Current.Resources["Red"];
                        largeIcon = await AssetManager.GetAsset("Icon/Delete");
                    }
                    break;
                case ViewType.RichPresence:
                    {
                        //throw new NotImplementedException();
                    }
                    break;
            }
            bdrContainer.Background = bgColour;

            tblTitle.Text = title;

            tblLine1.Text = text1;
            tblLine1.Visibility = string.IsNullOrEmpty(text1) ? Visibility.Collapsed : Visibility.Visible;

            tblLine2.Text = text2;
            tblLine2.Visibility = string.IsNullOrEmpty(text2) ? Visibility.Collapsed : Visibility.Visible;

            UpdateIcon(regLarge, ((Image)largeIcon)?.Source);
            UpdateIcon(elpSmall, ((Image)smallIcon)?.Source);
            grdSmall.Visibility = smallIcon == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateIcon(Shape shape, ImageSource source)
        {
            shape.Fill = source != null ? new ImageBrush
            {
                ImageSource = source
            } : null;
            shape.Visibility = source != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
