using JetBrains.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiRPC.Core
{
    /// <summary>
    /// A Custom Profile for <see cref="Rpc"/>
    /// </summary>
    public class CustomProfile : INotifyPropertyChanged
    {
        private string largeKey = "";
        /// <summary>
        /// The key of the image to use for the main image
        /// </summary>
        [CanBeNull]
        public string LargeKey
        {
            get => largeKey;
            set
            {
                if (largeKey == value)
                {
                    return;
                }

                largeKey = value;
                OnPropertyChanged();
            }
        }

        private string largeText;
        /// <summary>
        /// Text to show when hovering over <see cref="LargeKey"/>
        /// </summary>
        [CanBeNull]
        public string LargeText
        {
            get => largeText;
            set
            {
                if (largeText == value)
                {
                    return;
                }

                largeText = value;
                OnPropertyChanged();
            }
        }

        private bool showTime;
        /// <summary>
        /// If we should show the amount of time that the <see cref="Rpc.Rpc"/> started sending <see cref="DiscordRPC.RichPresence"/>'s
        /// </summary>
        public bool ShowTime
        {
            get => showTime;
            set
            {
                if (showTime == value)
                {
                    return;
                }

                showTime = value;
                OnPropertyChanged();
            }
        }

        private string smallKey;
        /// <summary>
        /// The key of the image to use for the small image
        /// </summary>
        [CanBeNull]
        public string SmallKey
        {
            get => smallKey;
            set
            {
                if (smallKey == value)
                {
                    return;
                }

                smallKey = value;
                OnPropertyChanged();
            }
        }

        private string smallText;
        /// <summary>
        /// Text to show when hovering over <see cref="SmallKey"/>
        /// </summary>
        [CanBeNull]
        public string SmallText
        {
            get => smallText;
            set
            {
                if (smallText == value)
                {
                    return;
                }

                smallText = value;
                OnPropertyChanged();
            }
        }

        //TODO: Readd this
        private string text1 = "";//LanguagePicker.GetLineFromLanguageFile("Hello");
        /// <summary>
        /// The first line of text to show
        /// </summary>
        [CanBeNull]
        public string Text1
        {
            get => text1;
            set
            {
                if (text1 == value)
                {
                    return;
                }

                text1 = value;
                OnPropertyChanged();
            }
        }

        private string text2 = "";//LanguagePicker.GetLineFromLanguageFile("World");
        /// <summary>
        /// The second line of text to show
        /// </summary>
        [CanBeNull]
        public string Text2
        {
            get => text2;
            set
            {
                if (text2 == value)
                {
                    return;
                }

                text2 = value;
                OnPropertyChanged();
            }
        }

        private string clientID = "";
        /// <summary>
        /// This profiles Application ID
        /// </summary>
        public string ClientID 
        {
            get => clientID;
            set 
            {
                if (clientID == value) 
                {
                    return;
                }

                clientID = value;
                OnPropertyChanged();
            }
        }

        private string name = "";
        /// <summary>
        /// The name of this <see cref="CustomProfile"/>
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                {
                    return;
                }

                name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}