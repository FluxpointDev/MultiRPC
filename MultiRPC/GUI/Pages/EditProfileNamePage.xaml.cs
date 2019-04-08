using System.Windows;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for EditProfileNamePage.xaml
    /// </summary>
    public partial class EditProfileNamePage : Page
    {
        public long WindowID;
        private Dictionary<string, CustomProfile> Profiles;
        private string OldName;

        public EditProfileNamePage(long windowID, Dictionary<string, CustomProfile> profiles, string oldName)
        {
            InitializeComponent();
            WindowID = windowID;
            Profiles = profiles;
            OldName = oldName;
            btnDone.Content = App.Text.Done;
            Title = App.Text.ProfileEdit;
            tbNewProfileName.Text = oldName;
        }

        private async void ButDone_OnClick(object sender, RoutedEventArgs e)
        {
            if (await UpdateProfileName(tbNewProfileName.Text))
                MainWindow.CloseWindow(WindowID, tbNewProfileName.Text);
        }

        private async Task<bool> UpdateProfileName(string newProfileName)
        {
            if (string.IsNullOrWhiteSpace(newProfileName))
            {
                await CustomMessageBox.Show(App.Text.EmptyProfileName + "!!!");
                return false;
            }

            if (!Profiles.ContainsKey(newProfileName))
            {
                var profile = Profiles[OldName];
                profile.Name = newProfileName;
                Profiles.Remove(OldName);
                Profiles.Add(newProfileName, profile);
                return true;
            }
            else
            {
                await CustomMessageBox.Show(App.Text.SameProfileName);
                return false;
            }
        }
    }
}
