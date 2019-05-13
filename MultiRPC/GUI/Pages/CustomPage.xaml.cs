using System;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using MultiRPC.Functions;
using System.Windows.Input;
using System.Windows.Media;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for CustomPage.xaml
    /// </summary>
    public partial class CustomPage : Page
    {
        private Image selectedHelpImage;
        public static Dictionary<string, CustomProfile> Profiles;
        public static List<Button> ProfileButtons = new List<Button>();
        public static Button CurrentButton;
        public static CustomPage _CustomPage;
        private bool haveDoneAutoStart;

        public CustomPage()
        {
            InitializeComponent();
            Loaded += CustomPage_Loaded;
            _CustomPage = this;

            if (File.Exists(FileLocations.ProfilesFileLocalLocation))
            {
                using (StreamReader reader = File.OpenText(FileLocations.ProfilesFileLocalLocation))
                    Profiles = (Dictionary<string, CustomProfile>)App.JsonSerializer.Deserialize(reader, typeof(Dictionary<string, CustomProfile>));
            }
            else
            {
                Profiles = new Dictionary<string, CustomProfile>
                {
                    { App.Text.Custom, new CustomProfile { Name = App.Text.Custom } }
                };
                using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
                    App.JsonSerializer.Serialize(writer, Profiles);
            }

            for (int i = 0; i < Profiles.Count; i++)
            {
                MakeMenuButton(Profiles.ElementAt(i).Key);
            }
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

        private Task UpdateText()
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
            RPC.UpdateType(RPC.RPCType.Custom);
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);

            while (tbProfiles.Items.Count < 0)
            {
                tbProfiles.Items.Refresh();
                await Task.Delay(520);
            }

            if (CurrentButton != null)
                CurrentButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            else
                ProfileButtons[App.Config.SelectedCustom].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;

            UpdateText();

            if (!haveDoneAutoStart && App.Config.AutoStart == App.Text.Custom)
            {
                if (await CanRunRPC(true))
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                haveDoneAutoStart = true;
            }
            else
            {
                CanRunRPC(true);
            }
        }

        private void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText(((TextBox)sender));
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText(((TextBox)sender));
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private async void TbText1_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            string text = await MultiRPCAndCustomLogic.CheckImageText((TextBox)sender);

            RPC.SetPresence(text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private async void TbText2_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            string text = await MultiRPCAndCustomLogic.CheckImageText((TextBox)sender);

            RPC.SetPresence(tbText1.Text, text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        public async void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            MultiRPCAndCustomLogic.UpdateTimestamps((CheckBox)sender);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
        }

        private void TbLargeKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
        }

        private void TbSmallKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
        }

        public async Task<bool> CanRunRPC(bool TokenTextChanged = false)
        {
            return await MultiRPCAndCustomLogic.CanRunRPC(tbText1, tbText2, tbSmallText, tbLargeText, tbClientID, TokenTextChanged);
        }

        //Custom Page Code
        private async void TbClientID_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text, cbElapasedTime.IsChecked.Value);
            CanRunRPC(true);
        }

        public static async Task JumpListLogic(string buttonName, bool fromStartUp = false)
        {
            if (fromStartUp)
            {
                while (MainPage._MainPage == null || MainPage._MainPage == null || MainPage._MainPage.gridCheckForDiscord.Visibility == Visibility.Visible)
                {
                    await Task.Delay(250);
                }
            }

            await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
            {
                MainPage._MainPage.btnCustom.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
            if (MainPage._MainPage.btnStart.Content.ToString() == App.Text.Shutdown)
            {
                await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                });
            }

            for (int i = 0; i < ProfileButtons.Count; i++)
            {
                if (ProfileButtons[i].Content.ToString() == buttonName)
                {
                    ProfileButtons[i].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (await _CustomPage.CanRunRPC(true))
                    {
                        await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
                        {
                            MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        });
                    }

                    break;
                }
            }
        }

        private async void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            imgProfileDelete.Visibility = Profiles[((Button)sender).Content.ToString()] == Profiles.Values.First()
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (CurrentButton != null)
                CurrentButton.Background = null;
            CurrentButton = (Button)sender;
            CurrentButton.SetResourceReference(Button.BackgroundProperty, "AccentColour2HoverSCBrush");
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
            Storyboard storyboard = new Storyboard();

            if (selectedHelpImage != null)
            {
                if (selectedHelpImage == (Image)sender)
                {
                    imgHelpImageBehind.Source = null;
                    Animations.ImageFadeAnimation(imgHelpImage, 0, storyboard);
                    Animations.ImageFadeAnimation(selectedHelpImage, 0.6);
                    Animations.ImageFadeAnimation(imgHelpImageBehind, 0);
                    selectedHelpImage = null;
                }
                else
                {
                    storyboard.Completed += ImageFaded;
                    Animations.ImageFadeAnimation(selectedHelpImage, 0.6);
                    Animations.ImageFadeAnimation(imgHelpImageBehind, 1);

                    imgHelpImageBehind.Source = ((Image)((Image)sender).Tag).Source;
                    Animations.ImageFadeAnimation(imgHelpImage, 0, storyboard);

                    selectedHelpImage = (Image)sender;
                    Animations.ImageFadeAnimation(selectedHelpImage, 1);
                }
            }
            else
            {
                imgHelpImage.Source = ((Image)((Image)sender).Tag).Source;
                Animations.ImageFadeAnimation(imgHelpImage, 1);
                selectedHelpImage = (Image)sender;
                Animations.ImageFadeAnimation(selectedHelpImage, 1);
            }
        }

        private void ImageFaded(object sender, EventArgs e)
        {
            Animations.ImageFadeAnimation(imgHelpImage, 1, duration: new Duration(TimeSpan.Zero));
            if (imgHelpImageBehind.Source != null || imgHelpImageBehind.Opacity < 1)
            {
                imgHelpImage.Source = imgHelpImageBehind.Source;
                imgHelpImageBehind.Source = null;
            }
        }

        private async Task ShowHelpImages()
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
        }

        private async Task UpdateProfile(string profileName, string text1, string text2, string largeKey, string largeText, string smallKey, string smallText, string clientID, bool showTime)
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
        }

        private async void ImgProfileEdit_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow window;
            var ticks = DateTime.Now.Ticks;
            var page = new EditProfileNamePage(ticks, Profiles, CurrentButton.Content.ToString());
            window = new MainWindow(page, false);
            window.WindowID = ticks;
            window.ShowDialog();

            if (window.ToReturn != null)
            {
                CurrentButton.Content = window.ToReturn;
                tblProfileName.Text = (string)window.ToReturn;
            }
            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }

            MainWindow.MakeJumpList();
        }

        private async void ImgProfileShare_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow window;
            var ticks = DateTime.Now.Ticks;
            var page = new ShareProfilePage(Profiles[CurrentButton.Content.ToString()], ticks);
            window = new MainWindow(page, false);
            window.WindowID = ticks;
            window.ShowDialog();
            if (window.ToReturn != null)
            {
                CustomProfile profile = null;
                try
                {
                    profile = JsonConvert.DeserializeObject<CustomProfile>(Utils.Base64Decode((string)window.ToReturn));
                }
                catch (Exception exception)
                {
                    await CustomMessageBox.Show(App.Text.SharingError);
                    App.Logging.Application($"{App.Text.SharingError}\r\n{App.Text.ExceptionMessage}{exception.Message}");
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

        string MakeProfileName(string name = null)
        {
            int keyCount = (Profiles.Count + 1);
            if(string.IsNullOrWhiteSpace(name))
                name = App.Text.Custom + keyCount;
            while (Profiles.ContainsKey(name))
            {
                keyCount++;
                name = App.Text.Custom + keyCount;
            }

            return name;
        }

        public async Task MakeMenuButton(string profileName)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Button b = new Button
                {
                    Content = profileName,
                    Margin = new Thickness(2.5, 0, 2.5, 0),
                    BorderThickness = new Thickness(0)
                };
                b.SetResourceReference(Button.StyleProperty, "DefaultButton");
                b.Click += CustomProfileButton_Click;
                ProfileButtons.Add(b);
            });
        }

        private async void ImgProfileAdd_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            string name = MakeProfileName();
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
                    ProfileButtons[ProfileButtons.Count - 1].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                });
            });

            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
                App.JsonSerializer.Serialize(writer, Profiles);
            MainWindow.MakeJumpList();
        }

        private void ImgProfileDelete_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            string currentKey = CurrentButton.Content.ToString();

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
            for (int i = 0; i < ProfileButtons.Count; i++)
            {
                if (ProfileButtons[i].Content.ToString() == lastKey)
                {
                    ProfileButtons[i].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                }
            }
            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
            MainWindow.MakeJumpList();
        }

        private void Img_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Animations.ImageFadeAnimation(((Image)sender), 1);
        }

        private void Img_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Animations.ImageFadeAnimation(((Image)sender), 0.6);
        }
    }
}
