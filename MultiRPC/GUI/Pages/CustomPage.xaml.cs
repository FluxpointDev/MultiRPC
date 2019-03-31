using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DiscordRPC;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using Newtonsoft.Json;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for CustomPage.xaml
    /// </summary>
    public partial class CustomPage : Page
    {
        private Image SelectedHelpImage;
        private Dictionary<string, CustomProfile> Profiles;
        public List<Button> ProfileButtons = new List<Button>();
        private Button CurrentButton;

        private static CustomPage _CustomPage;
        public static CustomPage customPage => _CustomPage;

        public CustomPage()
        {
            InitializeComponent();
            UpdateText();
            Loaded += CustomPage_Loaded;
            _CustomPage = this;

            if (File.Exists(FileLocations.ProfilesFileLocalLocation))
            {
                using (StreamReader reader = File.OpenText(FileLocations.ProfilesFileLocalLocation))
                    Profiles = (Dictionary<string, CustomProfile>)App.JsonSerializer.Deserialize(reader, typeof(Dictionary<string, CustomProfile>));
            }
            else
            {
                Task.Run(() =>
                {
                    Profiles = new Dictionary<string, CustomProfile>
                    {
                        { App.Text.Custom, new CustomProfile { Name = App.Text.Custom } }
                    };
                    using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
                    {
                        App.JsonSerializer.Serialize(writer, Profiles);
                    }
                });
            }

            foreach (var profile in Profiles)
            {
                MakeMenuButton(profile.Key);
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

        public async Task UpdateText()
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

            if(CurrentButton != null)
                CurrentButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            else
                ProfileButtons[App.Config.SelectedCustom].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;

            UpdateText();
            CanRunRPC();
        }

        private async void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckImageText(((TextBox)sender));
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private async void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckImageText(((TextBox)sender));
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        public async Task<string> CheckImageText(TextBox textBox)
        {
            string text = textBox.Text;
            if (!Checks.UnderAmountOfBytes(text, 128))
                textBox.Undo();
            return textBox.Text;
        }

        private void TbText1_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            string text = ((TextBox)sender).Text;

            if (!Checks.UnderAmountOfBytes(text, 128))
            {
                ((TextBox)sender).Undo();
                text = ((TextBox)sender).Text;
            }

            RPC.SetPresence(text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        public async Task CanRunRPC()
        {
            bool isEnabled = true;
            if (tbText2.Text.Length == 1)
            {
                tbText2.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbText2.ToolTip = new Controls.ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText2.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbText2.ToolTip = null;
            }
            if (tbText1.Text.Length == 1)
            {
                tbText1.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbText1.ToolTip = new Controls.ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText1.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbText1.ToolTip = null;
            }

            if (tbSmallText.Text.Length == 1)
            {
                tbSmallText.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbSmallText.ToolTip = new Controls.ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
            }
            else
            {
                tbSmallText.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbSmallText.ToolTip = null;
            }
            if (tbLargeText.Text.Length == 1)
            {
                tbLargeText.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbLargeText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
            }
            else
            {
                tbLargeText.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbLargeText.ToolTip = null;
            }

            ulong ID = 0;
            if (App.Config.CheckToken)
            {
                var isValidCode =
                    ulong.TryParse(tbClientID.Text, NumberStyles.Any, new NumberFormatInfo(), out ID);
                if (!isValidCode && tbClientID.Text.Length == 0)
                    isValidCode = true;
                if ((ID.ToString().Length != tbClientID.MaxLength || !isValidCode))
                {
                    RPC.IDToUse = 0;
                    tbClientID.BorderBrush = (SolidColorBrush) App.Current.Resources["Red"];
                    tbClientID.ToolTip = !isValidCode
                        ? new ToolTip(App.Text.ClientIDIsNotValid)
                        : new ToolTip(App.Text.ClientIDMustBe18CharactersLong);
                    isEnabled = false;
                }
                else
                {
                    tbClientID.BorderBrush = (SolidColorBrush)App.Current.Resources["Orange"];
                    tbClientID.ToolTip = new ToolTip(App.Text.CheckingClientID);
                    await Task.Delay(1000);
                    HttpResponseMessage T = null;
                    try
                    {
                        HttpClient Client = new HttpClient();
                        T = await Client.PostAsync("https://discordapp.com/api/oauth2/token/rpc",
                            new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                {"client_id", ID.ToString()}
                            }));
                    }
                    catch
                    {
                        if (Profiles[CurrentButton.Content.ToString()].ClientID == ID.ToString() && MainPage.mainPage.ContentFrame.Content is CustomPage)
                        {
                            App.Logging.Error("API", App.Text.DiscordAPIDown);
                            tbClientID.ToolTip = new ToolTip($"{App.Text.NetworkIssue}!");
                            tbClientID.BorderBrush = (SolidColorBrush) App.Current.Resources["Red"];
                            isEnabled = false;
                        }
                    }
                    
                    if (Profiles[CurrentButton.Content.ToString()].ClientID == ID.ToString() && MainPage.mainPage.ContentFrame.Content is CustomPage)
                    {
                        if (T.StatusCode == HttpStatusCode.BadRequest)
                        {
                            App.Logging.Error("API", App.Text.ClientIDIsNotValid);
                            tbClientID.ToolTip = new ToolTip(App.Text.ClientIDIsNotValid);
                            tbClientID.BorderBrush = (SolidColorBrush) App.Current.Resources["Red"];
                            isEnabled = false;
                        }
                        else if (T.StatusCode != HttpStatusCode.InternalServerError)
                        {
                            string Response = T.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            App.Logging.Error("API", $"{App.Text.APIError} {Response}");
                            tbClientID.ToolTip = new ToolTip($"{App.Text.APIIssue}!");
                            tbClientID.BorderBrush = (SolidColorBrush) App.Current.Resources["Red"];
                            isEnabled = false;
                        }
                        else
                        {
                            RPC.IDToUse = ID;
                            tbClientID.BorderBrush = (SolidColorBrush) App.Current.Resources["AccentColour4SCBrush"];
                            tbClientID.ToolTip = null;
                        }
                    }
                }
            }

            if (Profiles[CurrentButton.Content.ToString()].ClientID == (ID != 0 ? ID.ToString() : "") && 
                MainPage.mainPage.ContentFrame.Content is CustomPage && MainPage.mainPage.butStart.Content != App.Text.Shutdown)
                MainPage.mainPage.butStart.IsEnabled = isEnabled;
        }

        private void TbText2_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            string text = ((TextBox)sender).Text;

            if (!Checks.UnderAmountOfBytes(text, 128))
            {
                ((TextBox)sender).Undo();
                text = ((TextBox)sender).Text;
            }

            RPC.SetPresence(tbText1.Text, text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        public static async Task UpdateTimestamps(CheckBox checkBox)
        {
            if (checkBox.IsChecked.Value)
                RPC.Presence.Timestamps = new Timestamps();
            else
                RPC.Presence.Timestamps = null;
        }

        public async void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            await UpdateTimestamps((CheckBox)sender);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
        }

        //Custom Page logic that isn't in MultiRPCPage

        private async void TbClientID_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            await UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
            CanRunRPC();
        }

        private void TbLargeKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
        }

        private void TbSmallKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RPC.SetPresence(tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdateProfile(tblProfileName.Text, tbText1.Text, tbText2.Text, tbLargeKey.Text, tbLargeText.Text, tbSmallKey.Text, tbSmallText.Text, tbClientID.Text , cbElapasedTime.IsChecked.Value);
        }

        private async void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            imgProfileDelete.Visibility = Profiles[((Button)sender).Content.ToString()] == Profiles.Values.First()
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (CurrentButton != null)
                CurrentButton.Background = (SolidColorBrush)App.Current.Resources["AccentColour1SCBrush"];
            CurrentButton = (Button)sender;
            CurrentButton.Background = (SolidColorBrush)App.Current.Resources["AccentColour2HoverSCBrush"];
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
            if (SelectedHelpImage != null)
            {
                if (SelectedHelpImage == (Image)sender)
                {
                    imgHelpImage.Source = null;
                    SelectedHelpImage.Opacity = 0.6;
                    SelectedHelpImage = null;
                }
                else
                {
                    SelectedHelpImage.Opacity = 0.6;
                    imgHelpImage.Source = ((Image)((Image)sender).Tag).Source;
                    SelectedHelpImage = (Image) sender;
                    SelectedHelpImage.Opacity = 1;
                }
            }
            else
            {
                imgHelpImage.Source = ((Image)((Image)sender).Tag).Source;
                SelectedHelpImage = (Image)sender;
                SelectedHelpImage.Opacity = 1;
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

        private async Task MakeMenuButton(string profileName)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Button b = new Button();
                b.Content = profileName;
                b.Margin = new Thickness(2.5, 0, 2.5, 0);
                b.Style = (Style)App.Current.Resources["DefaultButton"];
                b.BorderThickness = new Thickness(0);
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
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }
        }

        private void ImgProfileDelete_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            string currentKey = CurrentButton.Content.ToString();
            string lastKey = "";
            foreach (var profile in ProfileButtons)
            {
                if (profile.Content.ToString() == currentKey)
                    break;
                else
                    lastKey = profile.Content.ToString();
            }
            Profiles.Remove(currentKey);
            ProfileButtons.Remove(CurrentButton);
            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }
            tbProfiles.Items.Refresh();
            foreach (var button in ProfileButtons)
            {
                if (button.Content.ToString() == lastKey)
                {
                    button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                }
            }
            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Img_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((Image) sender).Opacity = 1;
        }

        private void Img_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 0.6;
        }
    }
}
