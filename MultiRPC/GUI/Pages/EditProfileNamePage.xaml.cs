using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for EditProfileNamePage.xaml
    /// </summary>
    public partial class EditProfileNamePage : Page
    {
        private readonly string _oldName;
        private readonly Dictionary<string, CustomProfile> _profiles;
        private readonly long _windowID;
        private bool _closing;

        public EditProfileNamePage(long windowID, Dictionary<string, CustomProfile> profiles, string oldName)
        {
            InitializeComponent();
            _windowID = windowID;
            _profiles = profiles;
            _oldName = oldName;
            btnDone.Content = App.Text.Done;
            Title = App.Text.ProfileEdit;
            tbNewProfileName.Text = _oldName;
        }

        private async void ButDone_OnClick(object sender, RoutedEventArgs e)
        {
            if (await CanUpdateProfileName(tbNewProfileName.Text))
            {
                _closing = true;
                MainWindow.CloseWindow(_windowID, tbNewProfileName.Text);
            }
        }

        private async Task<bool> CanUpdateProfileName(string newProfileName)
        {
            if (string.IsNullOrWhiteSpace(newProfileName))
            {
                await CustomMessageBox.Show(App.Text.EmptyProfileName + "!!!", MainWindow.GetWindow(_windowID));
                return false;
            }

            if (!_profiles.ContainsKey(newProfileName))
            {
                var profile = _profiles[_oldName];
                profile.Name = newProfileName;
                _profiles.Remove(_oldName);
                _profiles.Add(newProfileName, profile);
                return true;
            }

            await CustomMessageBox.Show(App.Text.SameProfileName, MainWindow.GetWindow(_windowID));
            return false;
        }

        public void EditProfileNamePage_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !_closing)
            {
                btnDone.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }
    }
}