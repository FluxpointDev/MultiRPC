using MultiRPC.Data;
using MultiRPC.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiRPC.GUI.Pages;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ViewCustom.xaml
    /// </summary>
    public partial class ViewCustom : UserControl
    {
        public ViewCustom(CustomProfile profile)
        {
            Profile = profile;
            InitializeComponent();
            ProfileName.Content = profile.Name;
            TextClientID.Text = profile.ClientID;
            TextText1.Text = profile.Text1;
            TextText2.Text = profile.Text2;
            TextLargeKey.Text = profile.LargeKey;
            TextLargeText.Text = profile.LargeText;
            TextSmallKey.Text = profile.SmallKey;
            TextSmallText.Text = profile.SmallText;
            if (TextClientID.Text.Length < 15 || !ulong.TryParse(TextClientID.Text, out ulong ID))
            {
                HelpError.ToolTip = "Invalid client ID";
                HelpError.Visibility = Visibility.Visible;
            }
            else
                HelpError.Visibility = Visibility.Hidden;
            if (profile.Name == "Custom")
            {
                (MenuIcons.Items[2] as Image).Visibility = Visibility.Hidden;
                (MenuIcons.Items[2] as Image).Width = 0;
                (MenuIcons.Items[5] as Image).Visibility = Visibility.Hidden;
                (MenuIcons.Items[5] as Image).Width = 0;
            }
            if (App.Config.Disabled.HelpIcons)
                DisableHelpIcons();
        }

        public CustomProfile Profile;

        private void ProfileIcon_Click(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            switch(img.Name)
            {
                case "ProfileEdit":
                    {
                        RenameWindow PN = new RenameWindow(Profile)
                        {
                            Owner = App.BW
                        };
                        PN.ShowDialog();
                    }
                    break;
                case "ProfileShare":
                    {
                        ShareWindow PS = new ShareWindow(Profile)
                        {
                            Owner = App.BW
                        };
                        PS.ShowDialog();
                    }
                    break;
                case "ProfileAdd":
                    {
                        if (_Data.Profiles.Keys.Count == 30)
                            return;
                        int Count = 1;
                        string Name = $"Custom1";
                        while (_Data.Profiles.ContainsKey(Name))
                        {
                            Count++;
                            Name = $"Custom{Count}";
                        }
                        CustomProfile p = new CustomProfile { Name = Name };
                        _Data.Profiles.Add(p.Name, p);
                        _Data.SaveProfiles();
                        Button btn = p.GetButton();
                        btn.Click += MainPage.ProfileBtn_Click;
                        App.WD.MenuProfiles.Items.Add(btn);
                        App.WD.ToggleMenu();
                    }
                    break;
                case "ProfileDelete":
                    {
                        if (_Data.Profiles.Keys.Count == 1)
                            MessageBox.Show("Cannot delete the last custom profile");
                        else
                        {
                            int Index = 0;
                            bool Found = false;
                            foreach(object i in App.WD.MenuProfiles.Items)
                            {
                                Button b = i as Button;
                                if (b.Content == Profile.Name)
                                {
                                    Found = true;
                                    break;
                                }
                                Index++;
                            }
                            if (!Found)
                                return;
                            if (Index == 0)
                            {
                                MessageBox.Show("Cannot delete the first profile");
                                return;
                            }
                            App.WD.MenuProfiles.Items.RemoveAt(Index);
                            Views.Custom = new ViewCustom(_Data.Profiles.Values.First());
                            (App.WD.MenuProfiles.Items[0] as Button).Background = (Brush)Application.Current.Resources["Brush_Button"];
                            _Data.Profiles.Remove(Profile.Name);
                            _Data.SaveProfiles();
                            App.WD.ToggleMenu();
                            App.WD.FrameCustomView.Content = Views.Custom;
                        }
                    }
                    break;
            }
        }

        private void TextClientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            Profile.ClientID = (sender as TextBox).Text;
            if (Profile.ClientID.Length < 15 || !ulong.TryParse(Profile.ClientID, out ulong ID))
            {
                HelpError.ToolTip = "Invalid client ID";
                HelpError.Visibility = Visibility.Visible;
            }
            else
                HelpError.Visibility = Visibility.Hidden;

        }

        #region Set limit
        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Box = sender as TextBox;
            SetLimitNumber(Box);
            if (Box.Text.Length == 25)
                Box.Opacity = 0.80;
            else
                Box.Opacity = 1;
        }

        private void SetLimitNumber(TextBox box)
        {
            double db = 0.50;
            if (box.Text.Length == 25)
                db = 1;
            else if (box.Text.Length > 20)
                db = 0.90;
            else if (box.Text.Length > 15)
                db = 0.80;
            else if (box.Text.Length > 10)
                db = 0.70;
            else if (box.Text.Length > 5)
                db = 0.60;
            switch (box.Name)
            {
                case "TextText1":
                    Profile.Text1 = TextText1.Text;
                    LimitCustomText1.Content = 25 - box.Text.Length;
                    LimitCustomText1.Opacity = db;
                    break;
                case "TextText2":
                    Profile.Text2 = TextText2.Text;
                    LimitCustomText2.Content = 25 - box.Text.Length;
                    LimitCustomText2.Opacity = db;
                    break;
                case "TextLargeKey":
                    Profile.LargeKey = TextLargeKey.Text;
                    LimitCustomLargeKey.Content = 25 - box.Text.Length;
                    LimitCustomLargeKey.Opacity = db;
                    break;
                case "TextLargeText":
                    Profile.LargeText = TextLargeText.Text;
                    LimitCustomLargeText.Content = 25 - box.Text.Length;
                    LimitCustomLargeText.Opacity = db;
                    break;
                case "TextSmallKey":
                    Profile.SmallKey = TextSmallKey.Text;
                    LimitCustomSmallKey.Content = 25 - box.Text.Length;
                    LimitCustomSmallKey.Opacity = db;
                    break;
                case "TextSmallText":
                    Profile.SmallText = TextSmallText.Text;
                    LimitCustomSmallText.Content = 25 - box.Text.Length;
                    LimitCustomSmallText.Opacity = db;
                    break;
            }
        }
        #endregion

        #region Toggle limit number
        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            SetLimitVisibility(box, Visibility.Visible);
        }

        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            SetLimitVisibility(box, Visibility.Hidden);
        }

        private void SetLimitVisibility(TextBox box, Visibility vis)
        {
            switch (box.Name)
            {
                case "TextText1":
                    LimitCustomText1.Visibility = vis;
                    break;
                case "TextText2":
                    LimitCustomText2.Visibility = vis;
                    break;
                case "TextLargeKey":
                    LimitCustomLargeKey.Visibility = vis;
                    break;
                case "TextLargeText":
                    LimitCustomLargeText.Visibility = vis;
                    break;
                case "TextSmallKey":
                    LimitCustomSmallKey.Visibility = vis;
                    break;
                case "TextSmallText":
                    LimitCustomSmallText.Visibility = vis;
                    break;
            }
        }
        #endregion

        private void HelpButton_Click(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img.Opacity == 1)
            {
                img.Opacity = 0.7;
                ImageHelp.Source = null;
                return;
            }
            HelpClientID.Opacity = 0.7;
            HelpText1.Opacity = 0.7;
            HelpText2.Opacity = 0.7;
            HelpLargeKey.Opacity = 0.7;
            HelpLargeText.Opacity = 0.7;
            HelpSmallKey.Opacity = 0.7;
            HelpSmallText.Opacity = 0.7;
            img.Opacity = 1;
            Uri Url = null;
            switch (img.Name)
            {
                case "HelpClientID":
                    Url = new Uri("https://i.imgur.com/QFO9nnY.png");
                    break;
                case "HelpText1":
                    Url = new Uri("https://i.imgur.com/WF0sOBx.png");
                    break;
                case "HelpText2":
                    Url = new Uri("https://i.imgur.com/loGpAh7.png");
                    break;
                case "HelpLargeKey":
                    Url = new Uri("https://i.imgur.com/UzHaAgw.png");
                    break;
                case "HelpLargeText":
                    Url = new Uri("https://i.imgur.com/CH9JmHG.png");
                    break;
                case "HelpSmallKey":
                    Url = new Uri("https://i.imgur.com/EoyRYhC.png");
                    break;
                case "HelpSmallText":
                    Url = new Uri("https://i.imgur.com/9CkGNiB.png");
                    break;
            }
            if (Url != null)
            {
                ImageSource imgSource = new BitmapImage(Url);
                ImageHelp.Source = imgSource;
            }
        }

        public void EnableHelpIcons()
        {
            HelpClientID.Visibility = Visibility.Visible;
            HelpText1.Visibility = Visibility.Visible;
            HelpText2.Visibility = Visibility.Visible;
            HelpLargeKey.Visibility = Visibility.Visible;
            HelpLargeText.Visibility = Visibility.Visible;
            HelpSmallKey.Visibility = Visibility.Visible;
            HelpSmallText.Visibility = Visibility.Visible;
        }

        public void DisableHelpIcons()
        {
            HelpClientID.Visibility = Visibility.Hidden;
            HelpText1.Visibility = Visibility.Hidden;
            HelpText2.Visibility = Visibility.Hidden;
            HelpLargeKey.Visibility = Visibility.Hidden;
            HelpLargeText.Visibility = Visibility.Hidden;
            HelpSmallKey.Visibility = Visibility.Hidden;
            HelpSmallText.Visibility = Visibility.Hidden;
        }
    }
}
