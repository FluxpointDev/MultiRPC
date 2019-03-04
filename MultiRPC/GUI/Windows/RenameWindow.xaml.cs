using MultiRPC.Data;
using System.Windows;
using System.Windows.Controls;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        public RenameWindow(CustomProfile profile)
        {
            InitializeComponent();
            Profile = profile;
            Title = $"Rename - {Profile.Name}";
        }
        private CustomProfile Profile;

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextName.Text))
            {
                MessageBox.Show("You need to enter some text!", "No text", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (_Data.Profiles.ContainsKey(TextName.Text))
            {
                MessageBox.Show("You already have a profile with this name", "Name conflict", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            IsEnabled = false;
            CustomProfile New = new CustomProfile

            {
                Name = TextName.Text,
                ClientID = Profile.ClientID,
                Text1 = Profile.Text1,
                Text2 = Profile.Text2,
                LargeKey = Profile.LargeKey,
                LargeText = Profile.LargeText,
                SmallKey = Profile.SmallKey,
                SmallText = Profile.SmallText
            };
            foreach (Button b in App.WD.MenuProfiles.Items)
            {
                if ((string)b.Content == Profile.Name)
                {
                    b.Content = New.Name;
                }
            }
            //foreach (Button b in App.WD.spTaskbarIcon.Children)
            //{
            //    if ((string)b.Content == Profile.Name)
            //    {
            //        b.Content = New.Name;
            //    }
            //}
            _Data.Profiles.Remove(Profile.Name);
            _Data.Profiles.Add(New.Name, New);
            _Data.SaveProfiles();
            Views.Custom = new ViewCustom(New);
            App.WD.FrameCustomView.Content = Views.Custom;
            Close();
        }
    }
}
