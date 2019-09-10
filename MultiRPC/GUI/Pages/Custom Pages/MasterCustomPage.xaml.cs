using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using MultiRPC.Functions;
using MultiRPC.GUI.Controls;
using MultiRPC.JsonClasses;
using Newtonsoft.Json;
using TabItem = MultiRPC.GUI.Controls.TabItem;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class MasterCustomPage : Page
    {
        public static Dictionary<string, CustomProfile> Profiles;
        public static List<Button> ProfileButtons = new List<Button>();
        public static Button CurrentButton;

        private TabPage _tabPage;
        public static MasterCustomPage _MasterCustomPage;
        public CustomPage CustomPage => (CustomPage) ((TabPage) _MasterCustomPage.frmContent.Content).Tabs[0].Page;

        public MasterCustomPage(double pageWidth)
        {
            InitializeComponent();

            LoadProfiles();
            _MasterCustomPage = this;
            Width = pageWidth;

            //TriggerWatch.Start();
            MakeJumpList();

            _tabPage = new TabPage(new[]
            {
                new TabItem
                {
                    TabName = App.Text.CustomPage,
                    Page = new CustomPage()
                },
                new TabItem
                {
                    TabName = App.Text.CustomTriggers,
                    Page = new TriggerPage(pageWidth)
                }
            });
            for (var i = 0; i < Profiles?.Count; i++)
            {
                MakeMenuButton(Profiles.ElementAt(i).Key);
            }

            tbProfiles.ItemsSource = ProfileButtons;
            tbProfiles.Items.Refresh();

            if (CurrentButton != null)
            {
                CurrentButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                ProfileButtons[App.Config.SelectedCustom].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }

            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;

            frmContent.Content = _tabPage;
        }

        private void LoadProfiles()
        {
            if (Profiles != null)
            {
                return;
            }

            if (File.Exists(FileLocations.ProfilesFileLocalLocation))
            {
                using (var reader = File.OpenText(FileLocations.ProfilesFileLocalLocation))
                {
                    Profiles = (Dictionary<string, CustomProfile>)App.JsonSerializer.Deserialize(reader,
                        typeof(Dictionary<string, CustomProfile>));
                }
            }
            else
            {
                Profiles = new Dictionary<string, CustomProfile>
                {
                    {App.Text.Custom, new CustomProfile {Name = App.Text.Custom}}
                };
                SaveProfiles();
            }
        }

        private string MakeProfileName(string name = null)
        {
            var keyCount = Profiles.Count + 1;
            if (String.IsNullOrWhiteSpace(name))
            {
                name = App.Text.Custom + keyCount;
            }

            while (Profiles.ContainsKey(name))
            {
                keyCount++;
                name = App.Text.Custom + keyCount;
            }

            return name;
        }

        private void MakeMenuButton(string profileName)
        {
            Dispatcher.Invoke(() =>
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
                b.Click += TriggerPage._TriggerPage.CustomProfileButton_Click;
                b.Click += CustomPage._CustomPage.CustomProfileButton_Click;
                ProfileButtons.Add(b);
            });
        }

        public Task UpdateText()
        {
            _tabPage.UpdateText(App.Text.CustomPage, App.Text.CustomTriggers);
            imgProfileEdit.ToolTip = new ToolTip(App.Text.ProfileEdit);
            imgProfileShare.ToolTip = new ToolTip(App.Text.ProfileShare);
            imgProfileAdd.ToolTip = new ToolTip(App.Text.ProfileAdd);
            imgProfileDelete.ToolTip = new ToolTip(App.Text.ProfileDelete);


            return Task.CompletedTask;
        }

        private async void CustomProfileButton_Click(object sender, RoutedEventArgs e)
        {
            _MasterCustomPage.imgProfileDelete.Visibility = Profiles[((Button)sender).Content.ToString()] == Profiles.Values.First()
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (CurrentButton != null)
            {
                CurrentButton.Background = null;
            }

            CurrentButton = (Button)sender;
            CurrentButton.SetResourceReference(Control.BackgroundProperty, "AccentColour2HoverSCBrush");
            var profile = Profiles[CurrentButton.Content.ToString()];
            _MasterCustomPage.tblProfileName.Text = profile.Name;

            App.Config.SelectedCustom = ProfileButtons.IndexOf((Button)sender);
            await App.Config.Save();
        }

        private async void ImgProfileEdit_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var ticks = DateTime.Now.Ticks;
            var page = new EditProfileNamePage(ticks, Profiles, CurrentButton.Content.ToString());
            var newProfileName = (string)await MainWindow.OpenWindow(
                page, true,
                ticks, false, (window) => { window.KeyDown += page.EditProfileNamePage_OnKeyDown; });

            if (!String.IsNullOrWhiteSpace(newProfileName))
            {
                CurrentButton.Content = newProfileName;
                tblProfileName.Text = newProfileName;

                SaveProfiles();
                MakeJumpList();
            }
        }

        private static Task MakeJumpList()
        {
            if (Profiles == null)
            {
                return Task.CompletedTask;
            }

            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                var jumpList = new JumpList();

                for (var i = 0; i < 10; i++)
                {
                    if (i > Profiles.Count - 1)
                    {
                        break;
                    }

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

        public static Task SaveProfiles()
        {
            using (var writer = new StreamWriter(FileLocations.ProfilesFileLocalLocation))
            {
                App.JsonSerializer.Serialize(writer, Profiles);
            }

            return Task.CompletedTask;
        }

        private async void ImgProfileShare_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            TopLine:
            var ticks = DateTime.Now.Ticks;
            var newProfile = (string)await MainWindow.OpenWindow(
                new ShareProfilePage(Profiles[CurrentButton.Content.ToString()], ticks), true, ticks,
                false);

            if (!String.IsNullOrWhiteSpace(newProfile))
            {
                CustomProfile profile = null;
                try
                {
                    newProfile = newProfile.Remove(0, 1);
                    newProfile = newProfile.Remove(newProfile.Length - 1, 1);
                    profile = JsonConvert.DeserializeObject<CustomProfile>(
                        Utils.Base64Decode(newProfile));
                }
                catch (Exception exception)
                {
                    await CustomMessageBox.Show(App.Text.SharingError);
                    App.Logging.Application(
                        $"{App.Text.SharingError}\r\n{App.Text.ExceptionMessage}{exception.Message}");
                    goto TopLine;
                }

                if (profile != null)
                {
                    profile.Name = MakeProfileName(profile.Name);
                    Profiles.Add(profile.Name, profile);
                    MakeMenuButton(profile.Name);
                    tbProfiles.Items.Refresh();
                    tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
                    SaveProfiles();
                }
            }
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
                MakeMenuButton(name);
                await Dispatcher.InvokeAsync(() =>
                {
                    tbProfiles.Items.Refresh();
                    tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
                    ProfileButtons[ProfileButtons.Count - 1].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                });
            });

            SaveProfiles();
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
            SaveProfiles();

            tbProfiles.Items.Refresh();
            for (var i = 0; i < ProfileButtons.Count; i++)
            {
                if (ProfileButtons[i].Content.ToString() == lastKey)
                {
                    ProfileButtons[i].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    break;
                }
            }

            tbProfiles.Visibility = tbProfiles.Items.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
            MakeJumpList();
        }

        private void Img_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            Animations.DoubleAnimation(image, 1, image.Opacity);
        }

        private void Img_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            Animations.DoubleAnimation(image, 0.6, image.Opacity);
        }
    }
}
