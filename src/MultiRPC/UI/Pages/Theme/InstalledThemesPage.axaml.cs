using System.ComponentModel;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Pages.Theme.Editor;

namespace MultiRPC.UI.Pages.Theme;

//TODO: Make it so we can load the preview UI on the fly
//TODO: Fix Active text not showing up sometimes
public partial class InstalledThemesPage : Grid, ITabPage
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
    private ActionDisposable<TextBlock>? _activeEditTextBlockDisposable;
    private ActionDisposable<TextBlock>? _activeActiveTextBlockDisposable;
    private ActionDisposable<TextBlock>? _activeShowingTextBlockDisposable;
    private Button _activeRemoveButton;

    public Language? TabName { get; } = LanguageText.InstalledThemes;
    public bool IsDefaultPage => false;
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        btnAdd.DataContext = (Language)LanguageText.AddTheme;
        btnAddAndApply.DataContext = (Language)LanguageText.AddAndApplyTheme;

        wppThemes.Children.AddRange(Themes.ThemeIndexes.Values.Select(x => MakePreviewUI(x)));
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
        LanguageGrab.LanguageChanged += (sender, args) =>
        {
            _activeEditTextBlockDisposable?.RunAction();
            _activeActiveTextBlockDisposable?.RunAction();
            _activeShowingTextBlockDisposable?.RunAction();
        };
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
                DataContext = (Language)LanguageText.NA,
                [!TextBlock.TextProperty] = new Binding("TextObservable^"),
            };
        }

        var editButton = new Button
        {
            DataContext = (Language)LanguageText.Edit,
            [!Button.ContentProperty] = new Binding("TextObservable^"),
            IsEnabled = !file?.StartsWith('#') ?? false,
            Tag = theme,
        };
        editButton.Click += EditButtonOnClick;
            
        var removeButton = new Button
        {
            DataContext = (Language)LanguageText.Remove,
            [!Button.ContentProperty] = new Binding("TextObservable^"),
            IsEnabled = editButton.IsEnabled,
            Tag = theme,
        };
        removeButton.Click += RemoveButtonOnClick;

        var cloneButton = new Button
        {
            DataContext = (Language)LanguageText.Clone,
            [!Button.ContentProperty] = new Binding("TextObservable^"),
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
            MakeActiveDisposable(themeNameText);
            removeButton.IsEnabled = false;
            _activeRemoveButton = removeButton;
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
        themeUI.PointerEntered += ThemeUIOnPointerEnter;
        themeUI.PointerExited += ThemeUIOnPointerLeave;
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
            _activeShowingTextBlockDisposable?.Dispose();
            _activeShowingTextBlockDisposable = null;
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

                _activeShowingTextBlockDisposable?.Dispose();
                _activeShowingTextBlockDisposable = MakeActionDisposable(th, 
                    tb => tb.Text = tb.Tag + " (" + Language.GetText(LanguageText.Showing) + ")", 
                    tb =>
                    {
                        tb.Text = tb.Tag.ToString();
                        _activeActiveTextBlockDisposable?.RunAction();
                        _activeEditTextBlockDisposable?.RunAction();
                    });
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

        _activeRemoveButton.IsEnabled = !((Theming.Theme?)_activeRemoveButton.Tag)?.Location?.StartsWith('#') ?? true;
        _activeRemoveButton = (Button)((StackPanel?)((StackPanel?)th.Parent)?.Children[0])?.Children[2]!;
        _activeRemoveButton.IsEnabled = false;
        
        MakeActiveDisposable(GetTextBlock(th));
    }

    private void MakeActiveDisposable(TextBlock textBlock)
    {
        _activeActiveTextBlockDisposable?.Dispose();
        _activeActiveTextBlockDisposable = new ActionDisposable<TextBlock>( 
            tb => tb.Text = tb.Tag + " (" + Language.GetText(LanguageText.Active) + ")", 
            tb =>
            {
                tb.Text = tb.Tag.ToString();
                if (tb == _activeEditTextBlockDisposable?.State)
                {
                    _activeEditTextBlockDisposable.RunAction();
                }
            }, textBlock);

        if (textBlock != _activeEditTextBlockDisposable?.State)
        {
            _activeActiveTextBlockDisposable.RunAction();
        }
        _activeEditTextBlockDisposable?.RunAction();
    }

    private void EditingThemeOnIsEditingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Theming.Theme.IsBeingEdited)
            && _activeEditTextBlockDisposable != null
            && (!_editingTheme?.IsBeingEdited ?? true))
        {
            _activeEditTextBlockDisposable.Dispose();
            _activeEditTextBlockDisposable = null;
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
        
        _activeEditTextBlockDisposable?.Dispose();
        _activeEditTextBlockDisposable = new ActionDisposable<TextBlock>(
            tb => tb.Text = tb.Tag + " (" + Language.GetText(LanguageText.Editing) + ")", 
            tb => tb.Text = tb.Tag.ToString(),  
            (TextBlock)((StackPanel?)st.Parent)?.Children[0]!);
        _activeEditTextBlockDisposable.RunAction();
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
    
    private TextBlock GetTextBlock(ThemePreview th) =>
        (TextBlock)((StackPanel?)((StackPanel?)th.Parent)?.Children[0])?.Children[0]!;

    private ActionDisposable<TextBlock> MakeActionDisposable(ThemePreview th, Action<TextBlock?> action, Action<TextBlock?> disposeAction)
    {
        var actionDis = new ActionDisposable<TextBlock>(action, disposeAction, GetTextBlock(th));
        actionDis.RunAction();
        return actionDis;
    }

    private async Task GetTheme(bool apply)
    {
        var files = await App.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = Language.GetText(LanguageText.MultiRPCThemes),
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType(Constants.ThemeFileExtension[1..]),
                new FilePickerFileType(Constants.LegacyThemeFileExtension[1..])
            }
        });
        if (!files.Any())
        {
            return;
        }

        foreach (var file in files)
        {
            if (!file.TryGetUri(out var fileUrl))
            {
                //TODO: Log
                continue;
            }
            var filePath = WebUtility.UrlDecode(fileUrl.AbsolutePath);
            
            var ext = Path.GetExtension(filePath);
            if (ext != Constants.ThemeFileExtension
                && ext != Constants.LegacyThemeFileExtension)
            {
                //TODO: Log
                continue;
            }

            Directory.CreateDirectory(Constants.ThemeFolder);
            var newThemeLoc = Path.Combine(Constants.ThemeFolder, Path.GetFileName(filePath));
            File.Copy(filePath, newThemeLoc);
        }

        if (apply && files[^1].TryGetUri(out var lastFileUrl))
        {
            var th = MultiRPC.Theming.Theme.Load(WebUtility.UrlDecode(lastFileUrl.AbsolutePath));
            if (th != null)
            {
                th.Apply();
                _activeTheme = th;
            }
        }
    }
}