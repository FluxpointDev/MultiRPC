using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for CustomPage.xaml
    /// </summary>
    public partial class CustomPage : Page
    {
        public static CustomPage _CustomPage;
        private bool _haveDoneAutoStart;
        private Image _selectedHelpImage;

        public CustomPage()
        {
            InitializeComponent();
            _CustomPage = this;
            UpdateText();

            imgSmallText.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/SmallTextHelp.jpg", UriKind.Relative))
            };
            imgLargeText.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/LargeTextHelp.jpg", UriKind.Relative))
            };
            imgText1.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/Text1Help.jpg", UriKind.Relative))
            };
            imgText2.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/Text2Help.jpg", UriKind.Relative))
            };
            imgClientID.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/ClientIDHelp.jpg", UriKind.Relative))
            };
            imgSmallKey.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/SmallAndLargeKeyHelp.jpg", UriKind.Relative))
            };
            imgLargeKey.Tag = new Image
            {
                Source = new BitmapImage(new Uri("../../../Assets/SmallAndLargeKeyHelp.jpg", UriKind.Relative))
            };

            _haveDoneAutoStart = !(App.Config.AutoStart == App.Text.Custom && !App.StartedWithJumpListLogic);
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

            return Task.CompletedTask;
        }

        private async void CustomPage_Loaded(object sender, RoutedEventArgs e)
        {
            ShowHelpImages().ConfigureAwait(false);

            if (!RPC.IsRPCRunning)
            {
                RPC.UpdateType(RPC.RPCType.Custom);
                RPC.SetPresence(MasterCustomPage.Profiles[MasterCustomPage._MasterCustomPage.tblProfileName.Text]);
            }

            if (!_haveDoneAutoStart)
            {
                if (await CanRunRPC(true))
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
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
            {
                RPC.SetPresence(MasterCustomPage.Profiles[MasterCustomPage._MasterCustomPage.tblProfileName.Text]);
            }
        }

        private void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);
            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, largeText: tbLargeText.Text);
            SetPresence();
            CanRunRPC();
        }

        private void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);
            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, smallText: tbSmallText.Text);
            SetPresence();
            CanRunRPC();
        }

        public void TbText1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, tbText1.Text);
            SetPresence();
            CanRunRPC();
        }

        private void TbText2_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, text2: tbText2.Text);
            SetPresence();
            CanRunRPC();
        }

        private void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            MultiRPCAndCustomLogic.UpdateTimestamps((CheckBox) sender);
            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, showTime: cbElapasedTime.IsChecked);
        }

        private void TbLargeKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, largeKey: tbLargeKey.Text);
            SetPresence();
        }

        private void TbSmallKey_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text, smallKey: tbSmallKey.Text);
            SetPresence();
        }

        private void TbClientID_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProfile(MasterCustomPage._MasterCustomPage.tblProfileName.Text,  clientID: tbClientID.Text);
            CanRunRPC(true);
        }

        public async Task<bool> CanRunRPC(bool tokenTextChanged = false)
        {
            return await MultiRPCAndCustomLogic.CanRunRPC(tbText1, tbText2, tbSmallText, tbLargeText, tbClientID,
                tokenTextChanged);
        }
        
        public static async Task StartCustomProfileLogic(string buttonName, bool fromStartUp = false)
        {
            if (fromStartUp)
            {
                while (MainPage._MainPage == null ||
                       MainPage._MainPage.gridCheckForDiscord.Visibility == Visibility.Visible)
                {
                    await Task.Delay(250);
                }
            }

            await MainPage._MainPage.Dispatcher.Invoke(async () =>
            {
                MainPage._MainPage.btnCustom.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                while (!(MainPage._MainPage.frmContent.Content is MasterCustomPage))
                {
                    await Task.Delay(250);
                }

                if (MainPage._MainPage.btnStart.Content != null &&
                    MainPage._MainPage.btnStart.Content.ToString() == App.Text.Shutdown)
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            });

            await MasterCustomPage._MasterCustomPage.Dispatcher.Invoke(async () =>
            {
                for (var i = 0; i < MasterCustomPage.ProfileButtons.Count; i++)
                {
                    if (MasterCustomPage.ProfileButtons[i].Content.ToString() == buttonName)
                    {
                        MasterCustomPage.ProfileButtons[i].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        if (await _CustomPage.CanRunRPC(true))
                        {
                            await MainPage._MainPage.Dispatcher.InvokeAsync(() =>
                            {
                                MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                            });
                        }
                        break;
                    }
                }
            });
        }

        public void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = MasterCustomPage.Profiles[((Button)sender).Content.ToString()];

            tbText1.Text = profile.Text1;
            tbText2.Text = profile.Text2;
            tbLargeText.Text = profile.LargeText;
            tbSmallText.Text = profile.SmallText;
            tbLargeKey.Text = profile.LargeKey;
            tbSmallKey.Text = profile.SmallKey;
            tbClientID.Text = profile.ClientID;
            cbElapasedTime.IsChecked = profile.ShowTime;
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

        private Task UpdateProfile(string profileName, string text1 = null, string text2 = null, string largeKey = null,
            string largeText = null, string smallKey = null, string smallText = null, string clientID = null, bool? showTime = null)
        {
            if (!IsLoaded)
            {
                return Task.CompletedTask;
            }
            
            var profile = MasterCustomPage.Profiles[profileName];
            profile.LargeKey = largeKey ?? profile.LargeKey;
            profile.Text1 = text1 ?? profile.Text1;
            profile.Text2 = text2 ?? profile.Text2;
            profile.LargeText = largeText ?? profile.LargeText;
            profile.SmallKey = smallKey ?? profile.SmallKey;
            profile.SmallText = smallText ?? profile.SmallText;
            profile.ShowTime = showTime ?? profile.ShowTime;
            profile.ClientID = clientID ?? profile.ClientID;
            MasterCustomPage.SaveProfiles();

            return Task.CompletedTask;
        }
    }
}