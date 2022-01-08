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
using MultiRPC.Extensions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme;

//TODO: Make it faster when loading in the theme
//TODO: Add Showing/Active/Editing Text
//TODO: Disable Remove button on active theme
//TODO: Add Edit button when theme editor has progress
public partial class InstalledThemes : UserControl, ITabPage
{
    //We keep a store of the active theme as we mess with it
    private Theming.Theme? _activeTheme;
    public Language? TabName { get; } = Language.GetLanguage(LanguageText.InstalledThemes);
    public bool IsDefaultPage => false;
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        btnAdd.DataContext = Language.GetLanguage(LanguageText.AddTheme);
        btnAddAndApply.DataContext = Language.GetLanguage(LanguageText.AddAndApplyTheme);
        
        wppThemes.Children.AddRange(
            new []
            {
                MakePreviewUI(Themes.Dark),
                MakePreviewUI(Themes.Light)
            });

        if (!Directory.Exists(Constants.ThemeFolder))
        {
            return;
        }

        _ = Task.Run(() =>
        {
            var files = Directory.GetFiles(Constants.ThemeFolder);
            MakePreviewUIs(files);
        });

        Theming.Theme.NewTheme += (sender, theme) =>
        {
            this.RunUILogic(() =>
            {
                var control = MakePreviewUI(theme, theme.Location);
                wppThemes.Children.Add(control);
            });
        };
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

    private async void MakePreviewUIs(string[] files)
    {
        foreach (var file in files)
        {
            /*Funnily enough, if we load in things too
             fast then it'll make the whole UI unresponsive*/
            await Task.Delay(50);
            var theme = Theming.Theme.Load(file);
            this.RunUILogic(() =>
            {
                var control = MakePreviewUI(theme, file);
                wppThemes.Children.Add(control);
            });
        }
    }

    private Control MakePreviewUI(Theming.Theme? theme, string? file = null)
    {
        if (theme == null)
        {
            //TODO: Make different UI for this
            return new TextBlock
            {
                DataContext = Language.GetLanguage(LanguageText.NA),
                [!TextBlock.TextProperty] = new Binding("TextObservable^"),
            };
        }

        var editButton = new Button
        {
            DataContext = Language.GetLanguage(LanguageText.Edit),
            [!ContentProperty] = new Binding("TextObservable^"),
            IsEnabled = !string.IsNullOrWhiteSpace(file),
            Tag = theme,
        };
        editButton.Click += EditButtonOnClick;
            
        var removeButton = new Button
        {
            DataContext = Language.GetLanguage(LanguageText.Remove),
            [!ContentProperty] = new Binding("TextObservable^"),
            IsEnabled = false,//editButton.IsEnabled,
            Tag = theme,
        };
        removeButton.Click += RemoveButtonOnClick;

        var cloneButton = new Button
        {
            DataContext = Language.GetLanguage(LanguageText.Clone),
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
        {
            Margin = new Thickness(0, 0, 15, 15)
        };
        themeUI.PointerEnter += ThemeUIOnPointerEnter;
        themeUI.PointerLeave += ThemeUIOnPointerLeave;
        themeUI.DoubleTapped += ThemeUIOnDoubleTapped;
        themeUI.AttachedToLogicalTree += (sender, args) => _ = Task.Run(theme.ReloadAssets);
        themeUI.DetachedFromLogicalTree += (sender, args) => _ = Task.Run(theme.UnloadAssets);
        return new StackPanel
        {
            Children =
            {
                controlStackPanel,
                themeUI,
            },
        };
    }

    private Theming.Theme? _tmpTheme;
    private void ThemeUIOnPointerLeave(object? sender, PointerEventArgs e)
    {
        //We want to restore the old theme if it didn't become the new active theme
        var th = (ThemePreview)sender!;
        if (_tmpTheme != null
            && _activeTheme != th.Theme)
        {
            _activeTheme?.Apply();
            _tmpTheme = null;
        }
    }

    private async void ThemeUIOnPointerEnter(object? sender, PointerEventArgs e)
    {
        await Task.Delay(500);
        var th = (ThemePreview)sender!;
            
        this.RunUILogic(() =>
        {
            if (_activeTheme != th.Theme
                && th.IsPointerOver)
            {
                _tmpTheme = th.Theme;
                th.Theme.Apply();
            }
        });
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
        var files = await openDia.ShowAsync(((App)Application.Current).DesktopLifetime?.MainWindow!);
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

            Directory.CreateDirectory(Constants.ThemeFolder);
            var newThemeLoc = Path.Combine(Constants.ThemeFolder, Path.GetFileName(file));
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