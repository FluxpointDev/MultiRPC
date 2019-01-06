using MultiRPC.Data;
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
using System.Windows.Shapes;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ProfileName.xaml
    /// </summary>
    public partial class ProfileName : Window
    {
        public ProfileName(CustomProfile profile)
        {
            InitializeComponent();
            Profile = profile;
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
                    b.Name = New.Name;
                }
            }
            _Data.Profiles.Remove(Profile.Name);
            _Data.Profiles.Add(New.Name, New);
            _Data.SaveProfiles();
            MainWindow.CustomPage = new CustomPage(New);
            App.WD.ViewCustomPage.Content = MainWindow.CustomPage;
            Close();
        }
    }
}
