using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme
{
    //TODO: Add Showing/Active/Editing Text
    //TODO: Disable Remove button on active theme
    //TODO: Add Edit button when theme editor has progress
    //TODO: Fix disposing issue (Modern theming with assets)
    public partial class InstalledThemes : UserControl, ITabPage
    {
        //We keep a store of the active theme as we mess with it
        private Theming.Theme? _activeTheme;
        public Language? TabName { get; } = new Language("InstalledThemes");
        public bool IsDefaultPage { get; }
        public void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);
            wppThemes.Children.AddRange(
                new Control[]
                {
                    MakePreviewUI(Themes.Dark),
                    MakePreviewUI(Themes.Light)
                });
            
            wppThemes.Children.AddRange(
                Directory.EnumerateFiles(_themeLocation)
                    .Select(MakePreviewUI));
        }

        private void OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            //Grab the current theme
            _activeTheme = Theming.Theme.ActiveTheme;
        }

        private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            //Restore the old theme if we haven't yet
            if (_activeTheme != Theming.Theme.ActiveTheme)
            {
                _activeTheme?.Apply();
            }
        }

        private async void BtnAdd_OnClick(object? sender, RoutedEventArgs e) => await GetTheme(false);
        private async void BtnAddAndApply_OnClick(object? sender, RoutedEventArgs e) => await GetTheme(true);

        private Control MakePreviewUI(string file) => MakePreviewUI(Theming.Theme.Load(file), file);
        private Control MakePreviewUI(Theming.Theme? theme, string? file = null)
        {
            if (theme == null)
            {
                //TODO: Make different UI for this
                return new TextBlock
                {
                    DataContext = new Language("NA"),
                    [!TextBlock.TextProperty] = new Binding("TextObservable^"),
                };
            }

            var editButton = new Button
            {
                DataContext = new Language("Edit"),
                [!ContentProperty] = new Binding("TextObservable^"),
                IsEnabled = !string.IsNullOrWhiteSpace(file),
                Tag = theme,
            };
            editButton.Click += EditButtonOnClick;
            
            var removeButton = new Button
            {
                DataContext = new Language("Remove"),
                [!ContentProperty] = new Binding("TextObservable^"),
                IsEnabled = editButton.IsEnabled,
                Tag = theme,
            };
            removeButton.Click += RemoveButtonOnClick;

            var cloneButton = new Button
            {
                DataContext = new Language("Clone"),
                [!ContentProperty] = new Binding("TextObservable^"),
                Tag = theme,
            };
            cloneButton.Click += CloneButtonOnClick;

            var controlStackPanel = new StackPanel
            {
                Children = 
                {
                    new TextBlock
                    {
                        Text = theme.Metadata.Name,
                        FontWeight = FontWeight.Light,
                        Classes = { "subtitle" }
                    },
                    editButton,
                    removeButton,
                    cloneButton,
                },
                Spacing = 5,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0,0,0,5),
            };

            var themeUI = new ThemePreview(theme)
            { Margin = new Thickness(0, 0, 15, 15) };
            themeUI.PointerEnter += ThemeUIOnPointerEnter;
            themeUI.PointerLeave += ThemeUIOnPointerLeave;
            themeUI.DoubleTapped += ThemeUIOnDoubleTapped;
            return new StackPanel
            {
                Children =
                {
                    controlStackPanel,
                    themeUI,
                },
            };
        }

        private void ThemeUIOnPointerLeave(object? sender, PointerEventArgs e)
        {
            //We want to restore the old theme if it didn't become the new active theme
            var th = (ThemePreview)sender!;
            if (_activeTheme != th.Theme)
            {
                _activeTheme?.Apply();
            }
        }

        private async void ThemeUIOnPointerEnter(object? sender, PointerEventArgs e)
        {
            await Task.Delay(1000);
            var th = (ThemePreview)sender!;
            if (_activeTheme != th.Theme 
                && th.IsPointerOver)
            {
                th.Theme.Apply();
            }
        }

        private readonly GeneralSettings _generalSettings = SettingManager<GeneralSettings>.Setting;
        private void ThemeUIOnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            //TODO: Add logic for official themes
            //Apply the theme and add it to the general setting
            var th = (ThemePreview)sender!;
            th.Theme.Apply();
            _activeTheme = th.Theme;
            _generalSettings.ThemeFile = th.Theme.Location;
        }

        private async void EditButtonOnClick(object? sender, RoutedEventArgs e)
        {
            //TODO: Add
            await MessageBox.Show(Language.GetText(LanguageText.ToAdd));
        }
        
        private async void CloneButtonOnClick(object? sender, RoutedEventArgs e)
        {
            var st = (Button)sender!;
            //TODO: Clone
            await MessageBox.Show(Language.GetText(LanguageText.ToAdd));
        }

        private void RemoveButtonOnClick(object? sender, RoutedEventArgs e)
        {
            var st = (Button)sender!;
            wppThemes.Children.Remove(st.Parent!.Parent);
            var theme = (Theming.Theme)st.Tag!;
            if (!string.IsNullOrWhiteSpace(theme.Location))
            {
                File.Delete(theme.Location);
            }
        }

        private readonly string _themeLocation = Path.Combine(Constants.SettingsFolder, "Themes");
        private async Task GetTheme(bool apply)
        {
            var openDia = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Name = "MultiRPC Themes",
                        Extensions = new List<string>(new []
                        {
                            Constants.ThemeFileExtension[1..],
                            Constants.LegacyThemeFileExtension[1..]
                        }),
                    }
                }
            };
            var files = await openDia.ShowAsync(((App)App.Current).DesktopLifetime?.MainWindow);
            if (files is null || !files.Any())
            {
                return;
            }

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
                var newThemeLoc = Path.Combine(_themeLocation, Path.GetFileName(file));
                File.Copy(file, newThemeLoc);
            }
            if (!apply)
            {
                return;
            }
            
            var th = Theming.Theme.Load(files[^1]);
            if (th != null)
            {
                th.Apply();
                _activeTheme = th;
            }
        }
    }
}