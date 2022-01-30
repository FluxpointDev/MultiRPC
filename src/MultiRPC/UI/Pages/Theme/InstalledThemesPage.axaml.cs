using System.Collections.Generic;
using System.ComponentModel;
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
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme;

//TODO: Fix Active text not showing up sometimes
public partial class InstalledThemesPage : UserControl, ITabPage
{
    public InstalledThemesPage()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
    }

    public InstalledThemesPage(ThemeEditorPage editorPage)
    {
        _editorPage = editorPage;
    }
    
    //We keep a store of the active theme as we mess with it
    private Theming.Theme? _activeTheme;
    private Theming.Theme? _tmpTheme;
    private Theming.Theme? _editingTheme;

    private readonly ThemeEditorPage _editorPage;
    private readonly GeneralSettings _generalSettings = SettingManager<GeneralSettings>.Setting;
    private TextBlock? _activeEditTextBlock;
    private TextBlock? _activeActiveTextBlock;
    private TextBlock? _activeShowingTextBlock;

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

        MultiRPC.Theming.Theme.NewTheme += (sender, theme) =>
        {
            this.RunUILogic(() =>
            {
                var control = MakePreviewUI(theme, theme.Location);
                wppThemes.Children.Add(control);
            });
        };
        Language.LanguageChanged += (sender, args) =>
        {
            ProcessShowingText();
            ProcessActiveText();
            ProcessEditText();
        };
    }
    
    private void ProcessShowingText()
    {
        if (_activeShowingTextBlock != null)
        {
            _activeShowingTextBlock.Text = _activeShowingTextBlock.Tag + " (" + Language.GetText(LanguageText.Showing) + ")";
        }
    }

    
    private void ProcessActiveText()
    {
        if (_activeActiveTextBlock != null)
        {
            _activeActiveTextBlock.Text = _activeActiveTextBlock.Tag + " (" + Language.GetText(LanguageText.Active) + ")";
        }
    }

    private void ProcessEditText()
    {
        if (_activeEditTextBlock != null)
        {
            _activeEditTextBlock.Text = _activeEditTextBlock.Tag + " (" + Language.GetText(LanguageText.Editing) + ")";
        }
    }
    
    private void OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        //Grab the current theme
        _activeTheme = MultiRPC.Theming.Theme.ActiveTheme;
    }

    private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        //Restore the old theme if we haven't yet
        if (_activeTheme != MultiRPC.Theming.Theme.ActiveTheme)
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
            var theme = Theming.Theme.ActiveTheme?.Location == file ? Theming.Theme.ActiveTheme : MultiRPC.Theming.Theme.Load(file);
            this.RunUILogic(() =>
            {
                var control = MakePreviewUI(theme, file);
                wppThemes.Children.Add(control);
            });
        }
    }

    private Control MakePreviewUI(MultiRPC.Theming.Theme? theme, string? file = null)
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
            IsEnabled = !file?.StartsWith('#') ?? false,
            Tag = theme,
        };
        editButton.Click += EditButtonOnClick;
            
        var removeButton = new Button
        {
            DataContext = Language.GetLanguage(LanguageText.Remove),
            [!ContentProperty] = new Binding("TextObservable^"),
            IsEnabled = editButton.IsEnabled,
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

        var isActiveTheme = theme == Theming.Theme.ActiveTheme;
        var themeNameText = new TextBlock
        {
            Text = theme.Metadata.Name + (isActiveTheme ? $" ({Language.GetText(LanguageText.Active)})" : ""),
            Tag = theme.Metadata.Name,
            FontWeight = FontWeight.Light,
            Classes = { "subtitle" }
        };

        if (isActiveTheme)
        {
            _activeActiveTextBlock = themeNameText;
        }
        var controlStackPanel = new StackPanel
        {
            Children = 
            {
                themeNameText,
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

    private void ThemeUIOnPointerLeave(object? sender, PointerEventArgs e)
    {
        //We want to restore the old theme if it didn't become the new active theme
        var th = (ThemePreview)sender!;
        if (_tmpTheme != null
            && _activeTheme != th.Theme)
        {
            _activeTheme?.Apply();
            _tmpTheme = null;
            if (_activeShowingTextBlock != null)
            {
                _activeShowingTextBlock.Text = _activeShowingTextBlock.Tag?.ToString();
            }
            _activeShowingTextBlock = null;
            ProcessEditText();
        }
    }

    private TextBlock GetTextBlockFromThemePreview(ThemePreview th) => (TextBlock)((StackPanel?)((StackPanel?)th.Parent)?.Children[0])?.Children[0]!;
    
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

                if (_activeShowingTextBlock != null)
                {
                    _activeShowingTextBlock.Text = _activeShowingTextBlock.Tag?.ToString();
                }
                _activeShowingTextBlock = GetTextBlockFromThemePreview(th);
                ProcessShowingText();
                ProcessActiveText();
            }
        });
    }
    
    private void ThemeUIOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        //Apply the theme and add it to the general setting
        var th = (ThemePreview)sender!;
        th.Theme.Apply();
        _activeTheme = th.Theme;
        _generalSettings.ThemeFile = th.Theme.Location;

        if (_activeActiveTextBlock != null)
        {
            _activeActiveTextBlock.Text = _activeActiveTextBlock.Tag?.ToString();
        }
        _activeActiveTextBlock = GetTextBlockFromThemePreview(th);
        ProcessActiveText();
        ProcessEditText();
    }

    private void EditingThemeOnIsEditingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Theming.Theme.IsBeingEdited)
            && _activeEditTextBlock != null
            && (!_editingTheme?.IsBeingEdited ?? true))
        {
            _activeEditTextBlock.Text = _activeEditTextBlock.Tag?.ToString();
            _activeEditTextBlock = null;
            ProcessActiveText();
        }
    }

    private void EditButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (_editingTheme != null)
        {
            _editingTheme.PropertyChanged -= EditingThemeOnIsEditingChanged;
        }

        var st = (Button)sender!;
        var th = (MultiRPC.Theming.Theme)st.Tag!;
        _editingTheme = th;
        _editorPage.EditTheme(_editingTheme);
        _editingTheme.PropertyChanged += EditingThemeOnIsEditingChanged;
        
        if (_activeEditTextBlock != null)
        {
            _activeEditTextBlock.Text = _activeEditTextBlock.Tag?.ToString();
        }
        _activeEditTextBlock = (TextBlock)((StackPanel)st.Parent).Children[0]!;
        ProcessEditText();
        ProcessActiveText();
    }
    
    private void CloneButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var st = (Button)sender!;
        var theme = ((MultiRPC.Theming.Theme)st.Tag!).Clone();
        theme.Save(theme.Metadata.Name);
    }

    private void RemoveButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var st = (Button)sender!;
        wppThemes.Children.Remove(st.Parent!.Parent);
        var theme = (MultiRPC.Theming.Theme)st.Tag!;
        if (File.Exists(theme.Location))
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
        var files = await openDia.ShowAsync();
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
            
        var th = MultiRPC.Theming.Theme.Load(files[^1]);
        if (th != null)
        {
            th.Apply();
            _activeTheme = th;
        }
    }
}