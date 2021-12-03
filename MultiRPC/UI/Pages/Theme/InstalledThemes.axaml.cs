using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme
{
    public partial class InstalledThemes : UserControl, ITabPage
    {
        public Language? TabName { get; } = new Language("InstalledThemes");
        public bool IsDefaultPage { get; }
        public void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);
            wppThemes.Children.AddRange(Directory.EnumerateFiles(_themeLocation).Select(MakePreviewUI));
        }

        private async void BtnAdd_OnClick(object? sender, RoutedEventArgs e)
        {
            await GetTheme(false);
        }

        private async void BtnAddAndApply_OnClick(object? sender, RoutedEventArgs e)
        {
            await GetTheme(true);
        }

        private Control MakePreviewUI(string file)
        {
            return new ThemePreview()
            {
                Margin = new Thickness(0, 0, 15, 15)
            };
        }

        private readonly string _themeLocation = Path.Combine(Constants.SettingsFolder, "Themes");
        private async Task GetTheme(bool apply)
        {
            var openDia = new OpenFileDialog();
            var files = await openDia.ShowAsync(((App)App.Current).DesktopLifetime?.MainWindow);
            if (files is null || !files.Any())
            {
                return;
            }

            var lastCopiedTheme = "";
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (ext != Constants.ThemeFileExtension
                    && ext != Constants.LegacyThemeFileExtension)
                {
                    //TODO: Log
                    continue;
                }

                Directory.CreateDirectory(_themeLocation);
                lastCopiedTheme = Path.Combine(_themeLocation, Path.GetFileName(file));
                File.Copy(file, lastCopiedTheme);
            }

            if (apply)
            {
                var th = Theming.Theme.Load(lastCopiedTheme);
                if (th != null)
                {
                    th.Apply();
                    return;
                }
                
                //TODO: Log
            }
        }
    }
}