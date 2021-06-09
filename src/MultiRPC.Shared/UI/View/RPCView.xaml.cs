using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using static MultiRPC.Core.LanguagePicker;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RPCView : LocalizablePage
    {
        public RPCView()
        {
            this.InitializeComponent();
        }

        public enum ViewType
        {
            Default,
            Default2,
            Loading,
            Error,
            RichPresence
        }

        public override void UpdateText()
        {
        }

        public RichPresence RichPresence { get; set; }

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
        
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value ?? RichPresence?.Name ?? "";
                OnPropertyChanged();
            }
        }

        private string text1;
        public string Text1
        {
            get => text1;
            set
            {
                text1 = value ?? RichPresence?.Presence.Details ?? "";
                OnPropertyChanged();
            }
        }

        private string text2;
        public string Text2
        {
            get => text2;
            set
            {
                text2 = value ?? RichPresence?.Presence.State;
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
                    ImageSource = new BitmapImage
                    {
                        //UriSource = RichPresence?.Presence.Assets?.LargeImageUri
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
                    ImageSource = new BitmapImage
                    {
                        //UriSource = RichPresence?.Assets?.SmallImage?.Uri
                    }
                };
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged()
        {
        }
    }
}
