using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MultiRPC.Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Notification;

namespace MultiRPC.Core
{
    /// <summary>
    /// Settings that MultiRPC provides
    /// </summary>
    public partial class Settings : INotifyPropertyChanged
    {
        private bool canSave = false;

        private Settings()
        {
            Rpc.Rpc.ConnectionOpened += (sender, msg) =>
            {
                LastUser = $"{msg.User.Username}#{msg.User.Discriminator:0000}";
            };
            MultiRPC.PropertyChanged += MultiRPC_PropertyChanged;
        }

        /// <summary>
        /// If we should enable debugging functions
        /// </summary>
        public bool Debug { get; } = true;

        /// <summary>
        /// Settings that clients should modify
        /// </summary>
        [NotNull]
        public static Settings Current { get; } = Load();

        /// <summary>
        /// Fires when the language in <see cref="Current"/> changes
        /// </summary>
        [CanBeNull]
        public event EventHandler LanguageChanged;

        /// <summary>
        /// Fires when the language in <see cref="Current"/> changes
        /// </summary>
        [CanBeNull]
        public event EventHandler NavigationTooltipsStatusChanged;

        [CanBeNull]
        public event PropertyChangedEventHandler PropertyChanged;

        private string activeLanguage = CultureInfo.CurrentUICulture.Name.ToLower();
        /// <summary>
        /// What language the client should use
        /// </summary>
        [NotNull]
        public string ActiveLanguage
        {
            get => activeLanguage;
            set
            {
                if (activeLanguage == value)
                {
                    return;
                }

                activeLanguage = value;
                LanguagePicker.LanguageJsonFileContent = JObject.Parse(File.ReadAllText(Path.Combine(Constants.LanguageFolder, (Current?.ActiveLanguage ?? "en-gb") + ".json")));
                LanguageChanged?.Invoke(null, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private string lastUser = "";
        /// <summary>
        /// What the last username that the user had was
        /// </summary>
        [CanBeNull]
        [JsonProperty] //Needed due to private set
        public string LastUser
        {
            get => lastUser;
            private set
            {
                if (lastUser == value)
                {
                    return;
                }

                lastUser = value;
                OnPropertyChanged();
            }
        }

        private bool afkTime = false;
        /// <summary>
        /// If Akf should show how long the user been afk for
        /// </summary>
        public bool AFKTime
        {
            get => afkTime;
            set 
            {
                if (afkTime == value)
                {
                    return;
                }

                afkTime = value;
                OnPropertyChanged();
            }
        }

        private string activeTheme = Path.Combine("Assets", "Themes", "DarkTheme" + Constants.ThemeFileExtension);
        /// <summary>
        /// What theme the client should use
        /// </summary>
        [NotNull]
        public string ActiveTheme 
        {
            get => activeTheme;
            set
            {
                if (activeTheme == value) 
                {
                    return;
                }

                activeTheme = value;
                OnPropertyChanged();
            }
        }

        private AutoStart autoStart = AutoStart.No;
        /// <summary>
        /// If we should start rich presence when app loads
        /// </summary>
        [NotNull]
        public AutoStart AutoStart
        {
            get => autoStart;
            set 
            {
                if (autoStart == value) 
                {
                    return;
                }

                autoStart = value;
                OnPropertyChanged();
            }
        }

        private bool autoUpdate = false;
        /// <summary>
        /// If we should auto update the client
        /// </summary>
        public bool AutoUpdate
        {
            get => autoUpdate;
            set
            {
                if (autoUpdate == value)
                {
                    return;
                }

                autoUpdate = value;
                OnPropertyChanged();
            }
        }

        private bool checkToken = true;
        /// <summary>
        /// If we should check the token before running <see cref="Rpc.Rpc.StartRpc"/>
        /// </summary>
        public bool CheckToken 
        {
            get => checkToken;
            set
            {
                if (checkToken == value)
                {
                    return;
                }

                checkToken = value;
                OnPropertyChanged();
            }
        }

        private DiscordClient clientToUse = DiscordClient.Any;
        /// <summary>
        /// What client we should connect to
        /// </summary>
        [NotNull]
        public DiscordClient ClientToUse
        {
            get => clientToUse;
            set
            {
                if (clientToUse == value)
                {
                    return;
                }

                clientToUse = value;
                OnPropertyChanged();
            }
        }

        private bool showHelpIcons = true;
        /// <summary>
        /// If the custom page should show the help icons
        /// </summary>
        public bool ShowHelpIcons 
        {
            get => showHelpIcons;
            set
            {
                if (showHelpIcons == value)
                {
                    return;
                }

                showHelpIcons = value;
                OnPropertyChanged();
            }
        }

        private bool discordCheck = true;
        /// <summary>
        /// Check if discord is running on the users device
        /// </summary>
        public bool DiscordCheck
        {
            get => discordCheck;
            set
            {
                if (discordCheck == value)
                {
                    return;
                }

                discordCheck = value;
                OnPropertyChanged();
            }
        }

        private bool hideTaskbarIconWhenMin = true;
        /// <summary>
        /// If the client should hide the from the taskbar when minimized
        /// </summary>
        public bool HideTaskbarIconWhenMin
        {
            get => hideTaskbarIconWhenMin;
            set
            {
                if (hideTaskbarIconWhenMin == value)
                {
                    return;
                }

                hideTaskbarIconWhenMin = value;
                OnPropertyChanged();
            }
        }

        private bool inviteWarn = false;
        /// <summary>
        /// If the user has been warned for invites in rich presence text
        /// </summary>
        public bool InviteWarn
        {
            get => inviteWarn;
            set
            {
                if (inviteWarn == value)
                {
                    return;
                }

                inviteWarn = value;
                OnPropertyChanged();
            }
        }

        private DefaultSettings multiRPC = new DefaultSettings();
        /// <summary>
        /// Default rich presence settings
        /// </summary>
        [NotNull]
        public DefaultSettings MultiRPC
        {
            get => multiRPC;
            set
            {
                if (multiRPC == value)
                {
                    return;
                }

                multiRPC.PropertyChanged -= MultiRPC_PropertyChanged;
                multiRPC = value;
                multiRPC.PropertyChanged += MultiRPC_PropertyChanged;
                OnPropertyChanged();
            }
        }

        private int selectedCustom = 0;
        /// <summary>
        /// Tells the client what <see cref="CustomProfile"/> to use
        /// </summary>
        public int SelectedCustom
        {
            get => selectedCustom;
            set
            {
                if (selectedCustom == value)
                {
                    return;
                }

                selectedCustom = value;
                OnPropertyChanged();
            }
        }

        private bool showNavigationTooltips = true;
        /// <summary>
        /// If we should show the tooltip for the navigation buttons
        /// </summary>
        public bool ShowNavigationTooltips 
        {
            get => showNavigationTooltips;
            set 
            {
                if (showNavigationTooltips == value)
                {
                    return;
                }

                showNavigationTooltips = value;
                OnPropertyChanged();
                NavigationTooltipsStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Get the settings stored on disk
        /// </summary>
        public static Settings Load()
        {
            if (!File.Exists(FileLocations.ConfigFileLocation))
            {
                return new Settings();
            }

            try
            {
                return Constants.JsonSerializer.Deserialize<Settings>(new JsonTextReader(new StringReader
                    (File.ReadAllText(FileLocations.ConfigFileLocation))));
            }
            catch (Exception ex)
            {
                NotificationCenter.Logger.Error(ex);
                return new Settings();
            }
        }

        /// <summary>
        /// Saves the users data onto the disk
        /// </summary>
        public void Save()
        {
            if (!canSave)
            {
                return;
            }

            using var file = File.CreateText(FileLocations.ConfigFileLocation);
            Constants.JsonSerializer.Serialize(file, this);
        }

        [OnDeserializing()]
        private void OnDeserializing(StreamingContext context)
        {
            canSave = false;
        }

        [OnDeserialized()]
        private void OnDeserialized(StreamingContext context)
        {
            canSave = true;
        }

        private void MultiRPC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged($"{nameof(DefaultSettings)}.{e.PropertyName}");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Save();
        }
    }
}
