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
        private long windowID;
        private Dictionary<string, CustomProfile> profiles;
        private string oldName;

        public EditProfileNamePage(long _windowID, Dictionary<string, CustomProfile> _profiles, string _oldName)
        {
            InitializeComponent();
            windowID = _windowID;
            profiles = _profiles;
            oldName = _oldName;
            btnDone.Content = App.Text.Done;
            Title = App.Text.ProfileEdit;
            tbNewProfileName.Text = oldName;
        }

        private async void ButDone_OnClick(object sender, RoutedEventArgs e)
        {
            if (await CanUpdateProfileName(tbNewProfileName.Text))
                MainWindow.CloseWindow(windowID, tbNewProfileName.Text);
        }

        private async Task<bool> CanUpdateProfileName(string newProfileName)
        {
            if (string.IsNullOrWhiteSpace(newProfileName))
            {
                await CustomMessageBox.Show(App.Text.EmptyProfileName + "!!!");
                return false;
            }

            if (!profiles.ContainsKey(newProfileName))
            {
                var profile = profiles[oldName];
                profile.Name = newProfileName;
                profiles.Remove(oldName);
                profiles.Add(newProfileName, profile);
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
