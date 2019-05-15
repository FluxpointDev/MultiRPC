using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using MultiRPC.JsonClasses;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for ThemeEditorPage.xaml
    /// </summary>
    public partial class ThemeEditorPage : Page
    {
        public static ThemeEditorPage _ThemeEditorPage;

        private readonly string _designerXamlFile = Path.Combine("Assets", "Themes", "DesignerTheme.xaml");

        private readonly List<string> _themeNames = new List<string>();

        //For checking previewing the theme everywhere
        private string _lastFrameMouseWasIn;

        //This is for when someone is editing a theme, we need to know their remove button to remove and add the theme and
        //We need the theme name to allow the theme name TextBox to not disable when that is inputted while editing that theme
        private Button _removeButtonForThemeBeingEdited;

        //Border that has the colour we are editing
        private Border _selectedBorder;

        //ThemeDictionary for the theme being made/edited and the ThemeNames for all the theme names (so we don't need to go into 100000 StackPanel layers every time we change lol)
        private ResourceDictionary _themeDictionary = new ResourceDictionary();
        private string _themeNameThatBeingEdited;
        private int _themeThatBeingEditedIntLocation = -1;

        public ThemeEditorPage()
        {
            InitializeComponent();
            _ThemeEditorPage = this;

            MakeThemeUIEditable();
            UpdateText(App.Text.Editing, App.Text.Active);

            var files = Directory.EnumerateFiles(Path.Combine("Assets", "Themes"))
                .Concat(Directory.EnumerateFiles(FileLocations.ThemesFolder)).ToArray();
            for (var i = 0; i < files.LongCount(); i++)
            {
                if (files[i] == _designerXamlFile) continue;
                MakeThemeUI(files[i], putActive: true).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            //Trigger the first border mouse down event to make it the selected border
            var mouseDownEvent =
                new MouseButtonEventArgs(Mouse.PrimaryDevice, (int) DateTime.Now.Ticks, MouseButton.Left)
                {
                    RoutedEvent = MouseDownEvent,
                    Source = this
                };
            borderColour1.RaiseEvent(mouseDownEvent);
        }

        private void ThemeEditorPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Give the theme a name if it doesn't have one 
            if (string.IsNullOrWhiteSpace(tbThemeBeingMadeName.Text))
                tbThemeBeingMadeName.Text = "Theme " + wpInstalledThemes.Children.Count + 1;
        }

        public Task UpdateText(string oldEditingWord, string oldActiveWord)
        {
            btnAddTheme.Content = App.Text.AddTheme;
            btnAddAndApplyTheme.Content = App.Text.AddAndApplyTheme;

            tblMakeTheme.Text = $"{App.Text.LetMakeTheme}!";
            tblMakeTheme.ToolTip =
                new ToolTip($"{App.Text.ShareThemePart1}\r\n{FileLocations.ThemesFolder} {App.Text.ShareThemePart2}!");

            tblThemeName.Text = $"{App.Text.ThemeName}: ";
            tblColour1.Text = $"{App.Text.Colour1}:";
            tblColour2.Text = $"{App.Text.Colour2}:";
            tblColour2Hover.Text = $"{App.Text.Colour2Hover}:";
            tblColour3.Text = $"{App.Text.Colour3}:";
            tblColour4.Text = $"{App.Text.Colour4}:";
            tblColour5.Text = $"{App.Text.Colour5}:";
            tblTextColour.Text = $"{App.Text.TextColour}:";
            tblDiscordButtonColour.Text = $"{App.Text.DisabledButtonColour}:";
            tblDiscordButtonTextColour.Text = $"{App.Text.DisabledButtonTextColour}:";
            tblSelectedPageColour.Text = $"{App.Text.SelectedPageColour}:";
            tblSelectedPageIconColour.Text = $"{App.Text.SelectedPageIconColour}:";
            btnResetTheme.Content = App.Text.ResetTheme;

            btnSaveTheme.Content = App.Text.SaveTheme;
            btnSaveAndApplyTheme.Content = App.Text.SaveAndApplyTheme;
            tblInstalledThemes.Text = App.Text.InstalledThemes;

            //Go in each themeStackPanel and update the buttons text            
#pragma warning disable 4014
            EditInstalledTheme(stackPanel =>
#pragma warning restore 4014
            {
                var themeName = (TextBlock) ((StackPanel) stackPanel.Children[0]).Children[0];
                var text = GetThemeNameFromTextBlock(themeName, oldEditingWord, oldActiveWord);

                if (text == Application.Current.Resources["ThemeName"].ToString())
                    text = text + $" ({App.Text.Active})";
                else if (!string.IsNullOrWhiteSpace(_themeNameThatBeingEdited) && text == _themeNameThatBeingEdited)
                    text = text + $" ({App.Text.Editing})";
                themeName.Text = text;

                var themeTopControls = (StackPanel) stackPanel.Children[0];
                for (var i = 0; i < themeTopControls.Children.Count; i++)
                {
                    if (!(themeTopControls.Children[i] is Button button)) continue;
                    switch (button.Name)
                    {
                        case "btnEdit":
                            button.Content = App.Text.Edit;
                            break;
                        case "btnRemove":
                            button.Content = App.Text.Remove;
                            break;
                        case "btnClone":
                            button.Content = App.Text.Clone;
                            break;
                    }
                }
            });

            return Task.CompletedTask;
        }

        private Task EditInstalledTheme(Action<StackPanel> action)
        {
            for (var j = 0; j < wpInstalledThemes.Children.Count; j++)
                action((StackPanel) wpInstalledThemes.Children[j]);

            return Task.CompletedTask;
        }

        private async Task AddThemeButtonLogic(bool apply)
        {
            var openFile = new OpenFileDialog
            {
                Filter = $"MultiRPC {App.Text.Theme} | *{Theme.ThemeExtension}",
                Title = App.Text.AddTheme,
                Multiselect =
                    !apply //This is because we wouldn't know what theme to apply if we had multiple themes sent our way
            };

            if (openFile.ShowDialog().Value)
            {
                for (var i = 0; i < openFile.FileNames.Length; i++)
                    File.Move(openFile.FileNames[i],
                        Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));

                var themeFiles = new List<string>();
                for (var i = 0; i < openFile.SafeFileNames.LongLength; i++)
                    themeFiles.Add(Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));

                await AddExternalTheme(apply, themeFiles.ToArray());
            }
        }

        private async Task AddExternalTheme(bool apply, string[] themeFiles)
        {
            if (apply)
            {
                var frame = await MakeThemeUI(themeFiles[0]);
                var doubleClickEvent =
                    new MouseButtonEventArgs(Mouse.PrimaryDevice, (int) DateTime.Now.Ticks, MouseButton.Left)
                    {
                        RoutedEvent = Control.MouseDoubleClickEvent,
                        Source = this
                    };
                frame?.RaiseEvent(doubleClickEvent);
            }
            else
            {
                for (var i = 0; i < themeFiles.Length; i++) await MakeThemeUI(themeFiles[i]);
            }
        }

        private async Task<Frame> SaveTheme()
        {
            //Null theme name because we don't need it anymore and trigger remove button to remove UI and file (if editing theme)
            _removeButtonForThemeBeingEdited?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            _themeNameThatBeingEdited = null;

            var themeFileContent = Theme.ResourceDictionaryToTheme(_themeDictionary);
            await Theme.Save(themeFileContent);
            var themeFile = Theme.ThemeFileLocation(themeFileContent);

            var frame = await MakeThemeUI(themeFile, _themeThatBeingEditedIntLocation);
            _themeThatBeingEditedIntLocation = -1;

            //Clear and reset the Theme UI that is shown when making a theme
            _removeButtonForThemeBeingEdited = null;
            tbThemeBeingMadeName.Clear();
            await MakeThemeUIEditable();
            frmThemeBeingMade.Content = new MainPageThumbnail(_themeDictionary);
            btnSaveTheme.IsEnabled = true;

            //Return the frame for other functions that need it  
            return frame;
        }

        /// <summary>
        ///     This updates the UI everywhere with the theme
        /// </summary>
        /// <returns>A great looking MultiRPC (I hope anyway...)</returns>
        public static async Task UpdateGlobalUI(string themeFile = null)
        {
            if (string.IsNullOrWhiteSpace(themeFile))
            {
                if (File.Exists(App.Config.ActiveTheme)) themeFile = App.Config.ActiveTheme;
                else themeFile = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);
                //We want to at least have a theme so this doesn't happen: https://1drv.ms/u/s!AhwsT7MDO4OvgtsoUYv7Tmq7KWDleA
            }

            //Get rid of the old and in with the new ~~theme~~
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(
                Theme.ThemeToResourceDictionary(await Theme.Load(themeFile)));
            Application.Current.Resources.MergedDictionaries.Add(
                (ResourceDictionary) XamlReader.Parse(File.ReadAllText(Path.Combine("Assets", "Icons.xaml"))));
            MainPage._MainPage.RerenderButtons();

            ((MainWindow) Application.Current.MainWindow).TaskbarIcon.TrayToolTip = new ToolTip(
                Application.Current.MainWindow.WindowState == WindowState.Minimized
                    ? App.Text.ShowMultiRPC
                    : App.Text.HideMultiRPC);

            //Update the Theme name TextBox by using ~~magic~~ itself
            if (_ThemeEditorPage != null)
            {
                var tmp = _ThemeEditorPage.tbThemeBeingMadeName.Text;
                _ThemeEditorPage.tbThemeBeingMadeName.Clear();
                _ThemeEditorPage.tbThemeBeingMadeName.Text = tmp;
            }

            GC.Collect();
        }

        private Task UpdateBordersBackground(ResourceDictionary theme)
        {
            borderColour1.Background = (SolidColorBrush) theme["AccentColour1SCBrush"];
            borderColour2.Background = (SolidColorBrush) theme["AccentColour2SCBrush"];
            borderColour2Hover.Background = (SolidColorBrush) theme["AccentColour2HoverSCBrush"];
            borderColour3.Background = (SolidColorBrush) theme["AccentColour3SCBrush"];
            borderColour4.Background = (SolidColorBrush) theme["AccentColour4SCBrush"];
            borderColour5.Background = (SolidColorBrush) theme["AccentColour5SCBrush"];
            borderTextColour.Background = (SolidColorBrush) theme["TextColourSCBrush"];
            borderDiscordButtonColour.Background = (SolidColorBrush) theme["DisabledButtonColour"];
            borderDiscordButtonTextColour.Background = (SolidColorBrush) theme["DisabledButtonTextColour"];
            borderSelectedPageColour.Background = (SolidColorBrush) theme["NavButtonBackgroundSelected"];
            borderSelectedPageIconColour.Background = (SolidColorBrush) theme["NavButtonIconColourSelected"];

            return Task.CompletedTask;
        }

        private async Task MakeThemeUIEditable(string themeFile = null)
        {
            if (string.IsNullOrWhiteSpace(themeFile))
                themeFile = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);

            //Get the theme's C O N T E N T and slap it onto the screen
            var theme = await Theme.Load(themeFile);
            var themeDictionary = Theme.ThemeToResourceDictionary(theme);
            frmThemeBeingMade.Content = new MainPageThumbnail(theme);

            UpdateBordersBackground(themeDictionary);

            //Add theme's colours to ThemeDictionary
            _themeDictionary = themeDictionary;

            //Set the theme's name
            tbThemeBeingMadeName.Clear();
            if (themeFile != Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension))
            {
                _themeNameThatBeingEdited = (string) themeDictionary["ThemeName"];
                tbThemeBeingMadeName.Text = _themeNameThatBeingEdited;
            }
            else
            {
                if (_themeNames.Count != 0)
                    tbThemeBeingMadeName.Text =
                        $"{themeDictionary["ThemeName"]} " + (wpInstalledThemes.Children.Count + 1);
            }

            GC.Collect();
        }

        private string GetThemeNameFromTextBlock(TextBlock textBlock, string editingWord = null,
            string activeWord = null, string showingWord = null)
        {
            if (string.IsNullOrWhiteSpace(editingWord))
                editingWord = App.Text.Editing;
            if (string.IsNullOrWhiteSpace(activeWord))
                activeWord = App.Text.Active;
            if (string.IsNullOrWhiteSpace(showingWord))
                showingWord = App.Text.Showing;

            var bracketIndex = textBlock.Text.LastIndexOf("(");
            var endBracketIndex = textBlock.Text.LastIndexOf(")");

            var text = textBlock.Text;
            var lastBracketContent = bracketIndex != -1 && endBracketIndex != -1
                ? textBlock.Text.Remove(0, bracketIndex + 1)
                    .Remove(endBracketIndex - bracketIndex - 1).Trim()
                : "";
            if (int.TryParse(lastBracketContent, NumberStyles.Any, new NumberFormatInfo(), out var number))
            {
                var numberBracket = $"({number})";
                var tmp = text.IndexOf(numberBracket) + numberBracket.Length;
                if (tmp != textBlock.Text.Length) text = text.Remove(tmp);
            }
            else if (bracketIndex != -1 && (lastBracketContent == editingWord ||
                                            lastBracketContent == activeWord ||
                                            lastBracketContent == showingWord))
            {
                text = text.Remove(bracketIndex).Trim();
            }

            return text;
        }

        private async Task SetThemeStatusInUI(string status, StackPanel themesStackPanel = null,
            Frame themesFrame = null, string[] doNotSetIfContains = null, bool removeActiveOnOtherTextBoxes = false,
            bool removeEditOnOtherTextBoxes = false)
        {
            //Go in each themeStackPanel and update the buttons text            
            await EditInstalledTheme(stackPanel =>
            {
                var themeName = (TextBlock) ((StackPanel) stackPanel.Children[0]).Children[0];
                var text = GetThemeNameFromTextBlock(themeName);

                if (themesStackPanel != null && themesStackPanel == stackPanel ||
                    themesFrame != null && themesFrame == stackPanel.Children[1])
                {
                    if (doNotSetIfContains != null &&
                        doNotSetIfContains.Contains(themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "")))
                        return;

                    text = text + $" ({status})";
                    text = text.Replace(" ()", "");
                    themeName.Text = text;
                }
                else if (removeActiveOnOtherTextBoxes &&
                         themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Active)
                {
                    themeName.Text = text;
                }
                else if (removeEditOnOtherTextBoxes &&
                         themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Editing)
                {
                    themeName.Text = text;
                }
            });
        }

        private async Task<Frame> MakeThemeUI(string themeFile, int themeLocation = -1, bool putActive = false)
        {
            //Make the StackPanel and frame the theme's UI will be in
            var themeStackPanel = new StackPanel
            {
                Margin = new Thickness(4)
            };
            var frame = new Frame();

            //Make theme that will be shown
            var theme = new MainPageThumbnail(await Theme.Load(themeFile));
            var dropShadow = new DropShadowEffect
            {
                BlurRadius = 10,
                Opacity = 0.3,
                ShadowDepth = 0
            };
            frame.Content = theme;
            frame.Effect = dropShadow;
            frame.Tag = themeFile;
            frame.MouseDoubleClick += Theme_MouseDoubleClick;
            frame.MouseEnter += Frame_MouseEnter;
            frame.MouseLeave += Frame_MouseLeave;

            var isEnabled = !themeFile.StartsWith(Path.Combine("Assets", "Themes"));
            var tblThemeName = new TextBlock
            {
                Text = theme.Resources["ThemeName"] +
                       (putActive && theme.Resources["ThemeName"].ToString() ==
                        Application.Current.Resources["ThemeName"].ToString()
                           ? $" ({App.Text.Active})"
                           : ""),
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.FromOpenTypeWeight(2),
                FontSize = 16,
                MaxWidth = isEnabled ? 131 : 135,
                TextWrapping = TextWrapping.Wrap
            };
            _themeNames.Add(tblThemeName.Text);

            //Make buttons and see if it allowed to be edited (if made by us then that would be a big fat nope)
            var editButton = new Button
            {
                Content = App.Text.Edit,
                Name = "btnEdit",
                Margin = new Thickness(5, 0, 5, 0),
                IsEnabled = isEnabled,
                VerticalAlignment = VerticalAlignment.Center
            };
            var removeButton = new Button
            {
                Content = App.Text.Remove,
                Name = "btnRemove",
                IsEnabled = isEnabled,
                VerticalAlignment = VerticalAlignment.Center
            };
            var cloneButton = new Button
            {
                Content = App.Text.Clone,
                Name = "btnClone",
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            removeButton.Click += RemoveButton_Click;
            editButton.Click += EditButton_Click;
            cloneButton.Click += CloneButton_Click;
            removeButton.Tag = themeStackPanel;
            editButton.Tag = new Tuple<Button, string>(removeButton, themeFile);
            cloneButton.Tag = themeFile;

            //Add buttons and name to it's own StackPanel
            var topStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };
            topStackPanel.Children.Add(tblThemeName);
            topStackPanel.Children.Add(editButton);
            topStackPanel.Children.Add(removeButton);
            topStackPanel.Children.Add(cloneButton);

            //Add everything to the themeStackPanel and then the one that shows all theme's 
            themeStackPanel.Children.Add(topStackPanel);
            themeStackPanel.Children.Add(frame);
            if (themeLocation == -1) wpInstalledThemes.Children.Add(themeStackPanel);
            else wpInstalledThemes.Children.Insert(themeLocation, themeStackPanel);

            return frame;
        }

        private void Frame_MouseLeave(object sender, MouseEventArgs e)
        {
            _lastFrameMouseWasIn = null;
            UpdateGlobalUI();
            SetThemeStatusInUI("", themesFrame: (Frame) sender,
                doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing});
        }

        private async void Frame_MouseEnter(object sender, MouseEventArgs e)
        {
            var lastFrameMouseWasIn = _lastFrameMouseWasIn = (string) ((Frame) sender).Tag;
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            if (lastFrameMouseWasIn == _lastFrameMouseWasIn)
            {
                UpdateGlobalUI(lastFrameMouseWasIn);
                SetThemeStatusInUI(App.Text.Showing, themesFrame: (Frame) sender,
                    doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing});
            }
        }

        private async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            _removeButtonForThemeBeingEdited = null;

            //Get theme's C O N T E N T
            var themeContent = await Theme.Load(((Button) sender).Tag.ToString());

            //Edit name to have copy so user's know it's the cloned one
            if (!themeContent.ThemeMetadata.ThemeName.Contains("("))
                themeContent.ThemeMetadata.ThemeName = themeContent.ThemeMetadata.ThemeName + " (1)";

            var tmpName = themeContent.ThemeMetadata.ThemeName;
            var copyCount = 1;
            while (_themeNames.Contains(tmpName))
            {
                tmpName = tmpName.Replace($"({copyCount})", "") + $"({copyCount + 1})";
                copyCount++;
            }

            themeContent.ThemeMetadata.ThemeName = tmpName;

            Theme.Save(themeContent);

            //Show it on the screen (that's always helpful)
            await MakeThemeUI(Theme.ThemeFileLocation(themeContent));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //Get content in edit button
            var editContent = (Tuple<Button, string>) ((Button) sender).Tag;
            btnSaveTheme.IsEnabled = Application.Current.Resources["ThemeName"].ToString() !=
                                     Path.GetFileNameWithoutExtension(editContent.Item2);

            //Remake ThemeDictionary, set the remove button and show it at the top (always helpful)
            _removeButtonForThemeBeingEdited = editContent.Item1;
            MakeThemeUIEditable(editContent.Item2);

            SetThemeStatusInUI("", doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing},
                removeEditOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Editing, (StackPanel) _removeButtonForThemeBeingEdited.Tag,
                doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing});
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            for (var j = 0; j < wpInstalledThemes.Children.Count; j++)
            {
                var sp = wpInstalledThemes.Children[j];
                if (sp == ((Button) sender).Tag)
                {
                    //Remove it off this planet
                    var themeName =
                        ((TextBlock) ((StackPanel) ((StackPanel) wpInstalledThemes.Children[j]).Children[0])
                            .Children[0]).Text;
                    if (themeName.IndexOf("(") != -1)
                        themeName = themeName.Remove(themeName.IndexOf("("));
                    wpInstalledThemes.Children.Remove((StackPanel) sp);
                    _themeNames.Remove(themeName.Trim());
                    if (!string.IsNullOrWhiteSpace(_themeNameThatBeingEdited)) _themeThatBeingEditedIntLocation = j;

                    //Delete it (tony stark i don't feel so good....)
                    File.Delete(((Frame) ((StackPanel) sp).Children[1]).Tag.ToString());

                    //Remove the StackPanel that was containing it if it's lonely
                    if (wpInstalledThemes.Children.Count == 0)
                    {
                        wpInstalledThemes.Children.Remove(wpInstalledThemes);
                        return;
                    }

                    return;
                }
            }
        }

        private void Theme_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.Config.ActiveTheme = ((Frame) sender).Tag.ToString();
            App.Config.Save();
            SetThemeStatusInUI("", removeActiveOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Active, themesFrame: (Frame) sender);
            UpdateGlobalUI();
        }

        private void Colour_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _selectedBorder?.SetResourceReference(Border.BorderBrushProperty, "AccentColour4SCBrush");

            _selectedBorder = (Border) sender;
            _selectedBorder.SetResourceReference(Border.BorderBrushProperty, "AccentColour5SCBrush");

            ccvcolourPicker.IsEnabled = true;
            ccvcolourPicker.SelectedColor = ((SolidColorBrush) ((Border) sender).Background).Color;
        }

        private void ColourPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!IsInitialized)
                return;

            string solidColorBrushName = null;
            string colorName = null;

            //Update the border with the colour the person wants
            switch (_selectedBorder.Name)
            {
                case "borderColour1":
                    _themeDictionary["AccentColour1SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["AccentColour1"] = e.NewValue.Value;
                    borderColour1.Background = (SolidColorBrush) _themeDictionary["AccentColour1SCBrush"];
                    solidColorBrushName = "AccentColour1SCBrush";
                    colorName = "AccentColour1";
                    break;
                case "borderColour2":
                    _themeDictionary["AccentColour2SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["AccentColour2"] = e.NewValue.Value;
                    borderColour2.Background = (SolidColorBrush) _themeDictionary["AccentColour2SCBrush"];
                    solidColorBrushName = "AccentColour2SCBrush";
                    colorName = "AccentColour2";
                    break;
                case "borderColour2Hover":
                    _themeDictionary["AccentColour2HoverSCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["AccentColour2Hover"] = e.NewValue.Value;
                    borderColour2Hover.Background = (SolidColorBrush) _themeDictionary["AccentColour2HoverSCBrush"];
                    solidColorBrushName = "AccentColour2HoverSCBrush";
                    colorName = "AccentColour2Hover";
                    break;
                case "borderColour3":
                    _themeDictionary["AccentColour3SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["AccentColour3"] = e.NewValue.Value;
                    borderColour3.Background = (SolidColorBrush) _themeDictionary["AccentColour3SCBrush"];
                    solidColorBrushName = "AccentColour3SCBrush";
                    colorName = "AccentColour3";
                    break;
                case "borderColour4":
                    _themeDictionary["AccentColour4SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["AccentColour4"] = e.NewValue.Value;
                    borderColour4.Background = (SolidColorBrush) _themeDictionary["AccentColour4SCBrush"];
                    solidColorBrushName = "AccentColour4SCBrush";
                    colorName = "AccentColour4";
                    break;
                case "borderColour5":
                    _themeDictionary["AccentColour5SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["AccentColour5"] = e.NewValue.Value;
                    borderColour5.Background = (SolidColorBrush) _themeDictionary["AccentColour5SCBrush"];
                    solidColorBrushName = "AccentColour5SCBrush";
                    colorName = "AccentColour5";
                    break;
                case "borderTextColour":
                    _themeDictionary["TextColourSCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    _themeDictionary["TextColour"] = e.NewValue.Value;
                    borderTextColour.Background = (SolidColorBrush) _themeDictionary["TextColourSCBrush"];
                    solidColorBrushName = "TextColourSCBrush";
                    colorName = "TextColour";
                    break;
                case "borderDiscordButtonColour":
                    _themeDictionary["DisabledButtonColour"] = new SolidColorBrush(e.NewValue.Value);
                    borderDiscordButtonColour.Background = (SolidColorBrush) _themeDictionary["DisabledButtonColour"];
                    solidColorBrushName = "DisabledButtonColour";
                    break;
                case "borderDiscordButtonTextColour":
                    _themeDictionary["DisabledButtonTextColour"] = new SolidColorBrush(e.NewValue.Value);
                    borderDiscordButtonTextColour.Background =
                        (SolidColorBrush) _themeDictionary["DisabledButtonTextColour"];
                    solidColorBrushName = "DisabledButtonTextColour";
                    break;
                case "borderSelectedPageColour":
                    _themeDictionary["NavButtonBackgroundSelected"] = new SolidColorBrush(e.NewValue.Value);
                    borderSelectedPageColour.Background =
                        (SolidColorBrush) _themeDictionary["NavButtonBackgroundSelected"];
                    solidColorBrushName = "NavButtonBackgroundSelected";
                    break;
                case "borderSelectedPageIconColour":
                    _themeDictionary["NavButtonIconColourSelected"] = new SolidColorBrush(e.NewValue.Value);
                    borderSelectedPageIconColour.Background =
                        (SolidColorBrush) _themeDictionary["NavButtonIconColourSelected"];
                    solidColorBrushName = "NavButtonIconColourSelected";
                    break;
            }

            //Update the test UI with the colour
            if (frmThemeBeingMade.Content != null)
                ((MainPageThumbnail) frmThemeBeingMade.Content).UpdateMergedDictionaries(solidColorBrushName,
                    new SolidColorBrush(e.NewValue.Value), colorName);
            else
                frmThemeBeingMade.Content = new MainPageThumbnail(_themeDictionary);
        }

        private async void BtnSaveAndApplyTheme_OnClick(object sender, RoutedEventArgs e)
        {
            var frame = await SaveTheme();

            //Click the frame programmatically
            var doubleClickEvent =
                new MouseButtonEventArgs(Mouse.PrimaryDevice, (int) DateTime.Now.Ticks, MouseButton.Left)
                {
                    RoutedEvent = Control.MouseDoubleClickEvent, Source = this
                };
            frame?.RaiseEvent(doubleClickEvent);
        }

        private void BtnSaveTheme_OnClick(object sender, RoutedEventArgs e)
        {
            SaveTheme();
        }

        private void TbCurrentThemeName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var buttonToTrigger =
                Application.Current.Resources["ThemeName"].ToString() ==
                _themeNameThatBeingEdited
                    ? btnSaveAndApplyTheme
                    : btnSaveTheme;

            //Check the theme name
            if (tbThemeBeingMadeName.Text.Length == 0)
            {
                tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbThemeBeingMadeName.ToolTip = new ToolTip($"{App.Text.ThemeNeedName}!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                buttonToTrigger.IsEnabled = false;
                return;
            }

            var newThemeName = tbThemeBeingMadeName.Text.Trim();
            var sameThemeName = true;
            if (_themeNameThatBeingEdited != null)
                sameThemeName = !_themeNameThatBeingEdited.Equals(newThemeName);

            if (_themeNames.Contains(newThemeName) && sameThemeName)
            {
                tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbThemeBeingMadeName.ToolTip = new ToolTip($"{App.Text.ThemeWithSameName}!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                buttonToTrigger.IsEnabled = false;
                return;
            }

            for (var i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
                if (newThemeName.Contains(Path.GetInvalidFileNameChars()[i]))
                {
                    tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                    tbThemeBeingMadeName.ToolTip = new ToolTip($"{App.Text.InvalidThemeName}!!!");
                    btnSaveAndApplyTheme.IsEnabled = false;
                    buttonToTrigger.IsEnabled = false;
                    return;
                }

            tbThemeBeingMadeName.ToolTip = null;
            tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
            btnSaveAndApplyTheme.IsEnabled = true;
            buttonToTrigger.IsEnabled = true;
            _themeDictionary["ThemeName"] = newThemeName;
        }

        private void BtnAddTheme_OnClick(object sender, RoutedEventArgs e)
        {
            AddThemeButtonLogic(false);
        }

        private void BtnAddAndApplyTheme_OnClick(object sender, RoutedEventArgs e)
        {
            AddThemeButtonLogic(true);
        }

        private void BtnResetTheme_OnClick(object sender, RoutedEventArgs e)
        {
            _removeButtonForThemeBeingEdited = null;

            //Get theme's C O N T E N T
            SetThemeStatusInUI("", removeEditOnOtherTextBoxes: true);
            MakeThemeUIEditable();
            btnSaveTheme.IsEnabled = true;
            frmThemeBeingMade.Content = new MainPageThumbnail(_themeDictionary);
        }

        private void ThemeEditorPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            wpInstalledThemes.MaxWidth = e.NewSize.Width;
        }
    }
}