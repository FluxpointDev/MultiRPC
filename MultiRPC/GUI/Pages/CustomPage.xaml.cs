using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using Newtonsoft.Json;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for CustomPage.xaml
    /// </summary>
    public partial class CustomPage : Page
    {
        public static Dictionary<string, CustomProfile> Profiles;
        public static List<Button> ProfileButtons = new List<Button>();
        public static Button CurrentButton;
        public static CustomPage _CustomPage;
        private bool _haveDoneAutoStart;
        private Image _selectedHelpImage;

        public CustomPage(double mainPageWidth)
        {
            InitializeComponent();
            _CustomPage = this;
            UpdateText();
            tbProfiles.MaxWidth = mainPageWidth;

            if (File.Exists(FileLocations.ProfilesFileLocalLocation))
            {
                using (var reader = File.OpenText(FileLocations.ProfilesFileLocalLocation))
                {
                    Profiles = (Dictionary<string, CustomProfile>) App.JsonSerializer.Deserialize(reader,
                        typeof(Dictionary<string, CustomProfile>));
                }
            }
            else
            {
                Profiles = new Dictionary<string, CustomProfile>
                {
                    {App.Text.Custom, new CustomProfile {Name = App.Text.Custom}}
                };
                using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
                {
                    App.JsonSerializer.Serialize(writer, Profiles);
                }
            }

            MakeJumpList();
            for (var i = 0; i < Profiles.Count; i++) MakeMenuButton(Profiles.ElementAt(i).Key);
            tbProfiles.ItemsSource = ProfileButtons;

            imgSmallText.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/SmallTextHelp.jpg", UriKind.Relative))
            };
            imgLargeText.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/LargeTextHelp.jpg", UriKind.Relative))
            };
            imgText1.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/Text1Help.jpg", UriKind.Relative))
            };
            imgText2.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/Text2Help.jpg", UriKind.Relative))
            };
            imgClientID.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/ClientIDHelp.jpg", UriKind.Relative))
            };
            imgSmallKey.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/SmallAndLargeKeyHelp.jpg", UriKind.Relative))
            };
            imgLargeKey.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../Assets/SmallAndLargeKeyHelp.jpg", UriKind.Relative))
            };
        }

        public Task UpdateText()
        {
            tblClientID.Text = App.Text.ClientID + ":";
            tblText1.Text = App.Text.Text1 + ":";
            tblText2.Text = App.Text.Text2 + ":";
            tblLargeKey.Text = App.Text.LargeKey + ":";
            tblLargeText.Text = App.Text.LargeText + ":";
            tblSmallKey.Text = App.Text.SmallKey + ":";
            tblSmallText.Text = App.Text.SmallText + ":";
            tblElapasedTime.Text = App.Text.ShowElapsedTime + ":";
            imgProfileEdit.ToolTip = new ToolTip(App.Text.ProfileEdit);
            imgProfileShare.ToolTip = new ToolTip(App.Text.ProfileShare);
            imgProfileAdd.ToolTip = new ToolTip(App.Text.ProfileAdd);
            imgProfileDelete.ToolTip = new ToolTip(App.Text.ProfileDelete);

            return Task.CompletedTask;
        }

        private async void CustomPage_Loaded(object sender, RoutedEventArgs e)
        {
            ShowHelpImages();

            if (!RPC.IsRPCRunning)
            {
                RPC.UpdateType(RPC.RPCType.Custom);
                RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text,
                    tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            }

            while (tbProfiles.Items.Count < 0)
            {
                tbProfiles.Items.Refresh();
                await Task.Delay(520);
            }

            if (CurrentButton != null)
                CurrentButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            else
                ProfileButtons[App.Config.SelectedCustom].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;

            if (!_haveDoneAutoStart && App.Config.AutoStart == App.Text.Custom && !App.StartedWithJumpListLogic)
            {
                if (await CanRunRPC(true))
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                CanRunRPC(true);
            }

            _haveDoneAutoStart = true;
        }

        private void SetPresence()
        {
            if (!RPC.IsRPCRunning || RPC.Type == RPC.RPCType.Custom)
                RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text,
                    tbSmallText.Text, cbElapasedTime.IsChecked.Value);
        }

        private void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);
            SetPresence();
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);
            SetPresence();
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        public async void TbText1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = await MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            SetPresence();
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private async void TbText2_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = await MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            SetPresence();
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            MultiRPCAndCustomLogic.UpdateTimestamps((CheckBox) sender);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
        }

        private void TbLargeKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetPresence();
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
        }

        private void TbSmallKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetPresence();
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
        }

        public async Task<bool> CanRunRPC(bool tokenTextChanged = false)
        {
            return await MultiRPCAndCustomLogic.CanRunRPC(tbText1, tbText2, tbSmallText, tbLargeText, tbClientID,
                tokenTextChanged);
        }

        //Custom Page Code
        private void TbClientID_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text,
                tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC(true);
        }

        public static async Task JumpListLogic(string buttonName, bool fromStartUp = false)
        {
            if (fromStartUp)
                while (MainPage._MainPage == null ||
                       MainPage._MainPage.gridCheckForDiscord.Visibility == Visibility.Visible)
                    await Task.Delay(250);

            await MainPage._MainPage.Dispatcher.Invoke(async () =>
            {
                MainPage._MainPage.btnCustom.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                while (!(MainPage._MainPage.frmContent.Content is CustomPage))
                    await Task.Delay(250);
            });
            if (MainPage._MainPage.btnStart.Content != null &&
                MainPage._MainPage.btnStart.Content.ToString() == App.Text.Shutdown)
                await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                });

            for (var i = 0; i < ProfileButtons.Count; i++)
                if (ProfileButtons[i].Content.ToString() == buttonName)
                {
                    ProfileButtons[i].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    if (await _CustomPage.CanRunRPC(true))
                        await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
                        {
                            MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        });

                    break;
                }
        }

        private static Task MakeJumpList()
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                var jumpList = new JumpList();

                for (var i = 0; i < 10; i++)
                {
                    if (i > Profiles.Count - 1)
                        break;

                    //Configure a new JumpTask
                    var jumpTask = new JumpTask
                    {
                        // Set the JumpTask properties.
                        ApplicationPath = FileLocations.MultiRPCStartLink,
                        Arguments = $"-custom,{Profiles.ElementAt(i).Key}",
                        IconResourcePath = FileLocations.MultiRPCStartLink,
                        Title = Profiles.ElementAt(i).Key,
                        Description = $"{App.Text.Load} '{Profiles.ElementAt(i).Key}'",
                        CustomCategory = App.Text.CustomProfiles
                    };
                    jumpList.JumpItems.Add(jumpTask);
                }

                JumpList.SetJumpList(Application.Current, jumpList);
            }

            return Task.CompletedTask;
        }


        private async void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            imgProfileDelete.Visibility = Profiles[((Button) sender).Content.ToString()] == Profiles.Values.First()
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (CurrentButton != null)
                CurrentButton.Background = null;
            CurrentButton = (Button) sender;
            CurrentButton.SetResourceReference(Control.BackgroundProperty, "AccentColour2HoverSCBrush");
            var profile = Profiles[CurrentButton.Content.ToString()];
            tblProfileName.Text = profile.Name;
            tbText1.Text = profile.Text1;
            tbText2.Text = profile.Text2;
            tbLargeText.Text = profile.LargeText;
            tbSmallText.Text = profile.SmallText;
            tbLargeKey.Text = profile.LargeKey;
            tbSmallKey.Text = profile.SmallKey;
            tbClientID.Text = profile.ClientID;
            cbElapasedTime.IsChecked = profile.ShowTime;

            App.Config.SelectedCustom = ProfileButtons.IndexOf((Button) sender);
            await App.Config.Save();
        }

        private void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var storyboard = new Storyboard();

            if (_selectedHelpImage != null)
            {
                if (_selectedHelpImage == (Image) sender)
                {
                    imgHelpImageBehind.Source = null;
                    Animations.DoubleAnimation(imgHelpImage, 0, imgHelpImage.Opacity, storyboard);
                    Animations.DoubleAnimation(_selectedHelpImage, 0.6, _selectedHelpImage.Opacity);
                    Animations.DoubleAnimation(imgHelpImageBehind, 0, imgHelpImageBehind.Opacity);
                    _selectedHelpImage = null;
                }
                else
                {
                    storyboard.Completed += ImageFaded;
                    Animations.DoubleAnimation(_selectedHelpImage, 0.6, _selectedHelpImage.Opacity);
                    Animations.DoubleAnimation(imgHelpImageBehind, 1, imgHelpImageBehind.Opacity);

                    imgHelpImageBehind.Source = ((Image) ((Image) sender).Tag).Source;
                    Animations.DoubleAnimation(imgHelpImage, 0, imgHelpImage.Opacity, storyboard);

                    _selectedHelpImage = (Image) sender;
                    Animations.DoubleAnimation(_selectedHelpImage, 1, _selectedHelpImage.Opacity);
                }
            }
            else
            {
                imgHelpImage.Source = ((Image) ((Image) sender).Tag).Source;
                Animations.DoubleAnimation(imgHelpImage, 1, imgHelpImage.Opacity);
                _selectedHelpImage = (Image) sender;
                Animations.DoubleAnimation(_selectedHelpImage, 1, _selectedHelpImage.Opacity);
            }
        }

        private void ImageFaded(object sender, EventArgs e)
        {
            Animations.DoubleAnimation(imgHelpImage, 1, imgHelpImage.Opacity, duration: new Duration(TimeSpan.Zero));
            if (imgHelpImageBehind.Source != null || imgHelpImageBehind.Opacity < 1)
            {
                imgHelpImage.Source = imgHelpImageBehind.Source;
                imgHelpImageBehind.Source = null;
            }
        }

        private Task ShowHelpImages()
        {
            var vis = !App.Config.Disabled.HelpIcons ? Visibility.Visible : Visibility.Collapsed;
            imgClientID.Visibility = vis;
            imgText1.Visibility = vis;
            imgText2.Visibility = vis;
            imgLargeKey.Visibility = vis;
            imgLargeText.Visibility = vis;
            imgSmallKey.Visibility = vis;
            imgSmallText.Visibility = vis;
            imgHelpImage.Visibility = vis;

            return Task.CompletedTask;
        }

        private Task UpdateProfile(string profileName, string text1, string text2, string largeKey,
            string largeText, string smallKey, string smallText, string clientID, bool showTime)
        {
            Profiles[profileName] = new CustomProfile
            {
                LargeKey = largeKey,
                Text1 = text1,
                Text2 = text2,
                LargeText = largeText,
                SmallKey = smallKey,
                SmallText = smallText,
                ShowTime = showTime,
                ClientID = clientID,
                Name = profileName
            };
            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }

            return Task.CompletedTask;
        }

        private async void ImgProfileEdit_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var ticks = DateTime.Now.Ticks;
            var newProfileName = (string) await MainWindow.OpenWindow(
                new EditProfileNamePage(ticks, Profiles, CurrentButton.Content.ToString()), true,
                ticks, false);

            if (!string.IsNullOrWhiteSpace(newProfileName))
            {
                CurrentButton.Content = newProfileName;
                tblProfileName.Text = newProfileName;
            }

            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }

            MakeJumpList();
        }

        private async void ImgProfileShare_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var ticks = DateTime.Now.Ticks;
            var newProfile = (string) await MainWindow.OpenWindow(
                new ShareProfilePage(Profiles[CurrentButton.Content.ToString()], ticks), true, ticks,
                false);

            if (!string.IsNullOrWhiteSpace(newProfile))
            {
                CustomProfile profile = null;
                try
                {
                    profile = JsonConvert.DeserializeObject<CustomProfile>(
                        Utils.Base64Decode(newProfile));
                }
                catch (Exception exception)
                {
                    await CustomMessageBox.Show(App.Text.SharingError);
                    App.Logging.Application(
                        $"{App.Text.SharingError}\r\n{App.Text.ExceptionMessage}{exception.Message}");
                }

                if (profile != null)
                {
                    profile.Name = MakeProfileName(profile.Name);
                    Profiles.Add(profile.Name, profile);
                    await MakeMenuButton(profile.Name);
                    tbProfiles.Items.Refresh();
                    tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
                    using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
                    {
                        App.JsonSerializer.Serialize(writer, Profiles);
                    }
                }
            }
        }

        private string MakeProfileName(string name = null)
        {
            var keyCount = Profiles.Count + 1;
            if (string.IsNullOrWhiteSpace(name))
                name = App.Text.Custom + keyCount;
            while (Profiles.ContainsKey(name))
            {
                keyCount++;
                name = App.Text.Custom + keyCount;
            }

            return name;
        }

        private async Task MakeMenuButton(string profileName)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var b = new Button
                {
                    Content = profileName,
                    Margin = new Thickness(2.5, 0, 2.5, 0),
                    BorderThickness = new Thickness(0),
                    IsEnabled = !RPC.IsRPCRunning
                };
                b.SetResourceReference(StyleProperty, "DefaultButton");
                b.Click += CustomProfileButton_Click;
                ProfileButtons.Add(b);
            });
        }

        private void ImgProfileAdd_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var name = MakeProfileName();
            Profiles.Add(name, new CustomProfile
            {
                Name = name
            });
            Task.Run(async () =>
            {
                await MakeMenuButton(name);
                await Dispatcher.InvokeAsync(() =>
                {
                    tbProfiles.Items.Refresh();
                    tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
                    ProfileButtons[ProfileButtons.Count - 1].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                });
            });

            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }

            MakeJumpList();
        }

        private void ImgProfileDelete_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentKey = CurrentButton.Content.ToString();

            var lastKey = Profiles.Count - 2 >= App.Config.SelectedCustom
                ? Profiles.ElementAt(App.Config.SelectedCustom + 1).Key
                : Profiles.ElementAt(Profiles.Count - 2).Key;

            Profiles.Remove(currentKey);
            ProfileButtons.Remove(CurrentButton);
            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }

            tbProfiles.Items.Refresh();
            for (var i = 0; i < ProfileButtons.Count; i++)
                if (ProfileButtons[i].Content.ToString() == lastKey)
                {
                    ProfileButtons[i].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    break;
                }

            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
            MakeJumpList();
        }

        private void Img_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image) sender;
            Animations.DoubleAnimation(image, 1, image.Opacity);
        }

        private void Img_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var image = (Image) sender;
            Animations.DoubleAnimation(image, 0.6, image.Opacity);
        }
    }
}