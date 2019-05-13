using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using MultiRPC.JsonClasses;
using System.Windows.Markup;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.Effects;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for ThemeEditorPage.xaml
    /// </summary>
    public partial class ThemeEditorPage : Page
    {
        //ThemeDictionary for the theme being made/edited and the ThemeNames for all the theme names (so we don't need to go into 100000 StackPanel layers every time we change lol)
        private ResourceDictionary ThemeDictionary = new ResourceDictionary();
        private List<string> ThemeNames = new List<string>();

        //Border that has the colour we are editing
        private Border SelectedBorder;

        //This is for when someone is editing a theme, we need to know their remove button to remove and add the theme and
        //We need the theme name to allow the theme name TextBox to not disable when that is inputted while editing that theme
        private Button RemoveButtonForThemeBeingEdited;
        private string ThemeNameThatBeingEdited;
        private int ThemeThatBeingEditedIntLocation = -1;

        //This page as a static so when we update the UI with the new theme the static Task can do it from anywhere in the program
        private static ThemeEditorPage themeEditor;

        //For checking previewing the theme everywhere
        private string LastFrameMouseWasIn;

        private static bool globalThemeBeingUpdated;

        public ThemeEditorPage()
        {
            InitializeComponent();
            themeEditor = this;

            MakeThemeUIEditable();

            var files = Directory.EnumerateFiles(Path.Combine("Assets", "Themes"))
                .Concat(Directory.EnumerateFiles(FileLocations.ThemesFolder)).ToArray();
            for (int i = 0; i < files.LongCount(); i++)
            {
                if (files[i] == "Assets\\Themes\\DesignerTheme.xaml") continue;
                MakeThemeUI(files[i]).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            //Trigger the first border mouse down event to make it the selected border
            MouseButtonEventArgs mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Left)
            {
                RoutedEvent = Control.MouseDownEvent,
                Source = this
            };
            borderColour1.RaiseEvent(mouseDownEvent);
        }

        private void ThemeEditorPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Give the theme a name if it doesn't have one 
            if (string.IsNullOrWhiteSpace(tbCurrentThemeName.Text))
                tbCurrentThemeName.Text = "Theme " + wpInstalledThemes.Children.Count + 1;

#pragma warning disable 4014
            EditInstalledTheme((stackpanel) =>
#pragma warning restore 4014
            {
                TextBlock themeName = (TextBlock)((StackPanel)stackpanel.Children[0]).Children[0];

                string text = GetThemeNameFromTextBlock(themeName);

                if (text == App.Current.Resources["ThemeName"].ToString())
                    text = text + $" ({App.Text.Active})";
                else if (!string.IsNullOrWhiteSpace(ThemeNameThatBeingEdited) && text == ThemeNameThatBeingEdited)
                    text = text + $" ({App.Text.Editing})";
                themeName.Text = text;
            });

            //Update the UI Text in case there was a lang change
            UpdateText();
        }

        private Task UpdateText()
        {
            btnAddTheme.Content = App.Text.AddTheme;
            btnAddAndApplyTheme.Content = App.Text.AddAndApplyTheme;

            tblMakeTheme.Text = $"{App.Text.LetMakeTheme}!";
            tblMakeTheme.ToolTip = new ToolTip($"{App.Text.ShareThemePart1}\r\n{FileLocations.ThemesFolder} {App.Text.ShareThemePart2}!");

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
            EditInstalledTheme((stackpanel) =>
            {
                StackPanel themeTopControls = (StackPanel)stackpanel.Children[0];
                for (int i = 0; i < themeTopControls.Children.Count; i++)
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
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private async Task EditInstalledTheme(Action<StackPanel> action)
        {
            for (int j = 0; j < wpInstalledThemes.Children.Count; j++)
                action((StackPanel)wpInstalledThemes.Children[j]);
        }

        private async Task AddThemeButtonLogic(bool apply)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = $"MultiRPC {App.Text.Theme} | *{Theme.ThemeExtension}";
            openFile.Title = App.Text.AddTheme;
            openFile.Multiselect = !apply; //This is because we wouldn't know what theme to apply if we had multiple themes sent our way

            if (openFile.ShowDialog().Value)
            {
                for (int i = 0; i < openFile.FileNames.Length; i++)
                    File.Move(openFile.FileNames[i], Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));

                List<string> themeFiles = new List<string>();
                for (int i = 0; i < openFile.SafeFileNames.LongLength; i++)
                    themeFiles.Add(Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));

                await AddExternalTheme(apply, themeFiles.ToArray());
            }
        }

        private async Task AddExternalTheme(bool apply, string[] themeFiles)
        {
            if (apply)
            {
                var frame = await MakeThemeUI(themeFiles[0]);
                MouseButtonEventArgs doubleClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Left);
                doubleClickEvent.RoutedEvent = Control.MouseDoubleClickEvent;
                doubleClickEvent.Source = this;
                frame?.RaiseEvent(doubleClickEvent);
            }
            else
            {
                for (int i = 0; i < themeFiles.Length; i++)
                {
                    await MakeThemeUI(themeFiles[i]);
                }
            }
        }

        private async Task<Frame> SaveTheme()
        {
            //Null theme name because we don't need it anymore and trigger remove button to remove UI and file (if editing theme)
            RemoveButtonForThemeBeingEdited?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ThemeNameThatBeingEdited = null;


            var themeFileContent = Theme.ResourceDictionaryToTheme(ThemeDictionary);
            await Theme.Save(themeFileContent);
            var themeFile = Theme.ThemeFileLocation(themeFileContent);

            Frame frame;
            if (ThemeThatBeingEditedIntLocation == -1) frame = await MakeThemeUI(themeFile);
            else frame = await MakeThemeUI(themeFile, ThemeThatBeingEditedIntLocation);

            ThemeThatBeingEditedIntLocation = -1;
            //Clear and reset the Theme UI that is shown when making a theme
            RemoveButtonForThemeBeingEdited = null;
            tbCurrentThemeName.Clear();
            await MakeThemeUIEditable();
            frameThemeBeingMade.Content = new MainPageThumbnail(ThemeDictionary);

            
            //Return the frame for other functions that need it  
            return frame;
        }

        /// <summary>
        /// This updates the UI everywhere with the theme
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
            App.Current.Resources.MergedDictionaries.Clear();
            App.Current.Resources.MergedDictionaries.Add(Theme.ThemeToResourceDictionary(await Theme.Load(themeFile)));
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));
            MainPage._MainPage.RerenderButtons();

            ((MainWindow)App.Current.MainWindow).TaskbarIcon.TrayToolTip = new ToolTip(App.Current.MainWindow.WindowState == WindowState.Minimized ? App.Text.ShowMultiRPC : App.Text.HideMultiRPC);

            //Update the Theme name TextBox by using ~~magic~~ itself
            if (themeEditor != null)
            {
                string tmp = themeEditor.tbCurrentThemeName.Text;
                themeEditor.tbCurrentThemeName.Clear();
                themeEditor.tbCurrentThemeName.Text = tmp;
            }

            GC.Collect();
        }

        private Task UpdateBordersBackground(ResourceDictionary theme)
        {
            borderColour1.Background = ((SolidColorBrush)theme["AccentColour1SCBrush"]);
            borderColour2.Background = ((SolidColorBrush)theme["AccentColour2SCBrush"]);
            borderColour2Hover.Background = ((SolidColorBrush)theme["AccentColour2HoverSCBrush"]);
            borderColour3.Background = ((SolidColorBrush)theme["AccentColour3SCBrush"]);
            borderColour4.Background = ((SolidColorBrush)theme["AccentColour4SCBrush"]);
            borderColour5.Background = ((SolidColorBrush)theme["AccentColour5SCBrush"]);
            borderTextColour.Background = ((SolidColorBrush)theme["TextColourSCBrush"]);
            borderDiscordButtonColour.Background = ((SolidColorBrush)theme["DisabledButtonColour"]);
            borderDiscordButtonTextColour.Background = ((SolidColorBrush)theme["DisabledButtonTextColour"]);
            borderSelectedPageColour.Background = ((SolidColorBrush)theme["NavButtonBackgroundSelected"]);
            borderSelectedPageIconColour.Background = (SolidColorBrush)theme["NavButtonIconColourSelected"];
            return Task.CompletedTask;
        }

        private async Task MakeThemeUIEditable(string themeFile = "Assets/Themes/DarkTheme.multirpctheme")
        {
            Color getColor(object brush)
            {
                return (Color)brush;
            }

            //Get the theme's C O N T E N T and slap it onto the screen
            var theme = await Theme.Load(themeFile);
            var themeDictionary = Theme.ThemeToResourceDictionary(theme);
            frameThemeBeingMade.Content = new MainPageThumbnail(theme);

            UpdateBordersBackground(themeDictionary);

            ThemeDictionary = new ResourceDictionary();
            //Add theme's colours to ThemeDictionary
            ThemeDictionary.Add("AccentColour1SCBrush", themeDictionary["AccentColour1SCBrush"]);
            ThemeDictionary.Add("AccentColour2SCBrush", themeDictionary["AccentColour2SCBrush"]);
            ThemeDictionary.Add("AccentColour2HoverSCBrush", themeDictionary["AccentColour2HoverSCBrush"]);
            ThemeDictionary.Add("AccentColour3SCBrush", themeDictionary["AccentColour3SCBrush"]);
            ThemeDictionary.Add("AccentColour4SCBrush", themeDictionary["AccentColour4SCBrush"]);
            ThemeDictionary.Add("AccentColour5SCBrush", themeDictionary["AccentColour5SCBrush"]);
            ThemeDictionary.Add("TextColourSCBrush", themeDictionary["TextColourSCBrush"]);
            ThemeDictionary.Add("DisabledButtonColour", themeDictionary["DisabledButtonColour"]);
            ThemeDictionary.Add("DisabledButtonTextColour", themeDictionary["DisabledButtonTextColour"]);
            ThemeDictionary.Add("NavButtonBackgroundSelected", themeDictionary["NavButtonBackgroundSelected"]);
            ThemeDictionary.Add("NavButtonIconColourSelected", themeDictionary["NavButtonIconColourSelected"]);

            ThemeDictionary.Add("AccentColour1", getColor(themeDictionary["AccentColour1"]));
            ThemeDictionary.Add("AccentColour2", getColor(themeDictionary["AccentColour2"]));
            ThemeDictionary.Add("AccentColour2Hover", getColor(themeDictionary["AccentColour2Hover"]));
            ThemeDictionary.Add("AccentColour3", getColor(themeDictionary["AccentColour3"]));
            ThemeDictionary.Add("AccentColour4", getColor(themeDictionary["AccentColour4"]));
            ThemeDictionary.Add("AccentColour5", getColor(themeDictionary["AccentColour5"]));
            ThemeDictionary.Add("TextColour", getColor(themeDictionary["TextColour"]));
            ThemeDictionary.Add("ThemeName", themeDictionary["ThemeName"]);

            //Set the theme's name
            tbCurrentThemeName.Clear();
            if (themeFile != "Assets/Themes/DarkTheme" + Theme.ThemeExtension)
            {
                ThemeNameThatBeingEdited = (string)themeDictionary["ThemeName"];
                tbCurrentThemeName.Text = ThemeNameThatBeingEdited;
            }
            else
            {
                if(ThemeNames.Count != 0)
                    tbCurrentThemeName.Text = $"{themeDictionary["ThemeName"]} " + wpInstalledThemes.Children.Count + 1;
            }
            GC.Collect();
        }

        string GetThemeNameFromTextBlock(TextBlock textBlock)
        {
            int bracketIndex = textBlock.Text.LastIndexOf("(");
            int endBracketIndex = textBlock.Text.LastIndexOf(")");

            string text = textBlock.Text;
            string lastBracketContent = bracketIndex != -1 && endBracketIndex != -1 ? textBlock.Text.Remove(0, bracketIndex + 1)
                .Remove(endBracketIndex - bracketIndex - 1).Trim() : "";
            if (int.TryParse(lastBracketContent, NumberStyles.Any, new NumberFormatInfo(), out int number))
            {
                var numberBracket = $"({number})";
                var tmp = text.IndexOf(numberBracket) + numberBracket.Length;
                if (tmp != textBlock.Text.Length)
                {
                    text = text.Remove(tmp);
                }
            }
            else if (bracketIndex != -1 && (lastBracketContent == App.Text.Editing || lastBracketContent == App.Text.Active || lastBracketContent == App.Text.Showing))
            {
                text = text.Remove(bracketIndex).Trim();
            }

            return text;
        }

        private async Task SetThemeStatusInUI(string status, StackPanel themesStackPanel = null, Frame themesFrame = null, string[] dontSetIfContains = null, bool removeActiveOnOtherTextBoxes = false, bool removeEditOnOtherTextBoxes = false)
        {
            //Go in each themeStackPanel and update the buttons text            
            await EditInstalledTheme((stackpanel) =>
            {
                TextBlock themeName = (TextBlock)((StackPanel)stackpanel.Children[0]).Children[0];

                string text = GetThemeNameFromTextBlock(themeName);

                if ((themesStackPanel != null && themesStackPanel == stackpanel) ||
                    (themesFrame != null && themesFrame == stackpanel.Children[1]))
                {
                    if (dontSetIfContains != null && dontSetIfContains.Contains(themeName.Text.Split(' ').Last().Replace("(","").Replace(")", "")))
                        return;

                    text = text + $" ({status})";
                    text = text.Replace(" ()", "");
                    themeName.Text = text;
                }
                else if (removeActiveOnOtherTextBoxes)
                {
                    if (themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Active)
                    {
                        themeName.Text = text;
                    }
                }
                else if (removeEditOnOtherTextBoxes)
                {
                    if (themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Editing)
                    {
                        themeName.Text = text;
                    }
                }
            });
        }

        private async Task<Frame> MakeThemeUI(string themeFile, int themeLocation = -1)
        {
            var stackPanelToAddTo = wpInstalledThemes;

            //Make the StackPanel and frame the theme's UI will be in
            StackPanel themeStackPanel = new StackPanel
            {
                Margin = new Thickness(4)
            };
            var frame = new Frame();

            //Add theme to frame and make it look nice OwO
            var theme = new MainPageThumbnail(await Theme.Load(themeFile));
            frame.Content = theme;
            DropShadowEffect dropShadow = new DropShadowEffect
            {
                BlurRadius = 10,
                Opacity = 0.3,
                ShadowDepth = 0
            };
            frame.Effect = dropShadow;
            frame.Tag = themeFile;
            frame.MouseDoubleClick += Theme_MouseDoubleClick;
            frame.MouseEnter += Frame_MouseEnter;
            frame.MouseLeave += Frame_MouseLeave;

            var isEnabled = !themeFile.StartsWith(@"Assets\Themes");
            TextBlock tblThemeName = new TextBlock
            {
                Text = theme.Resources["ThemeName"].ToString(),
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.FromOpenTypeWeight(2),
                FontSize = 16,
                MaxWidth = isEnabled ? 131 : 135,
                TextWrapping = TextWrapping.Wrap
            };
            //Add it to the list of theme names if it somehow isn't
            if (!ThemeNames.Contains(tblThemeName.Text))
                ThemeNames.Add(tblThemeName.Text);

            //Make buttons and see if it allowed to be edited (if made by us then that would be a big fat nope)
            Button editButton = new Button
            {
                Content = App.Text.Edit,
                Name = "btnEdit",
                Margin = new Thickness(5,0,5,0),
                IsEnabled = isEnabled,
                VerticalAlignment = VerticalAlignment.Center
            };
            Button removeButton = new Button
            {
                Content = App.Text.Remove,
                Name = "btnRemove",
                IsEnabled = isEnabled,
                VerticalAlignment = VerticalAlignment.Center
            };
            Button cloneButton = new Button
            {
                Content = App.Text.Clone,
                Name = "btnClone",
                Margin = new Thickness(5,0,0,0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            removeButton.Click += RemoveButton_Click;
            editButton.Click += EditButton_Click;
            cloneButton.Click += CloneButton_Click;
            removeButton.Tag = themeStackPanel;
            editButton.Tag = new Tuple<Button, string>(removeButton, themeFile);
            cloneButton.Tag = themeFile;
            
            //Add buttons and name to it's own StackPanel
            StackPanel buttonAndNameStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };
            buttonAndNameStackPanel.Children.Add(tblThemeName);
            buttonAndNameStackPanel.Children.Add(editButton);
            buttonAndNameStackPanel.Children.Add(removeButton);
            buttonAndNameStackPanel.Children.Add(cloneButton);

            //Add everything to the themeStackPanel and then the one that shows all theme's 
            themeStackPanel.Children.Add(buttonAndNameStackPanel);
            themeStackPanel.Children.Add(frame);
            if (themeLocation == -1) stackPanelToAddTo.Children.Add(themeStackPanel);
            else stackPanelToAddTo.Children.Insert(themeLocation, themeStackPanel);

            return frame;
        }

        private void Frame_MouseLeave(object sender, MouseEventArgs e)
        {
            LastFrameMouseWasIn = null;
            UpdateGlobalUI();
            SetThemeStatusInUI("", themesFrame: ((Frame) sender), dontSetIfContains: new [] { App.Text.Active, App.Text.Editing });
        }

        private async void Frame_MouseEnter(object sender, MouseEventArgs e)
        {
            var lastFrameMouseWasIn = LastFrameMouseWasIn = (string)((Frame) sender).Tag;
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            if (lastFrameMouseWasIn == LastFrameMouseWasIn)
            {
                UpdateGlobalUI(lastFrameMouseWasIn);
                SetThemeStatusInUI(App.Text.Showing, themesFrame: ((Frame)sender), dontSetIfContains: new[] { App.Text.Active, App.Text.Editing });
            }
        }

        private async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveButtonForThemeBeingEdited = null;

            //Get theme's C O N T E N T
            var themeContent = await Theme.Load(((Button) sender).Tag.ToString());

            //Edit name to have copy so user's know it's the cloned one
            if (!themeContent.ThemeName.Contains("("))
                themeContent.ThemeName = themeContent.ThemeName + " (1)";

            var tmpName = themeContent.ThemeName;
            var copyCount = 1;
            while (ThemeNames.Contains(tmpName))
            {
                tmpName = tmpName.Replace($"({copyCount})", "") + $"({copyCount + 1})";
                copyCount++;
            }

            themeContent.ThemeName = tmpName;

            Theme.Save(themeContent);

            //Show it on the screen (that's always helpful)
            await MakeThemeUI(Theme.ThemeFileLocation(themeContent));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //Get content in edit button
            Tuple<Button, string> editContent = (Tuple<Button, string>) ((Button) sender).Tag;

            //Remake ThemeDictionary, set the remove button and show it at the top (always helpful)
            RemoveButtonForThemeBeingEdited = editContent.Item1;
            MakeThemeUIEditable(editContent.Item2);
            SetThemeStatusInUI("", dontSetIfContains: new[] { App.Text.Active, App.Text.Editing }, removeEditOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Editing, themesStackPanel: (StackPanel)RemoveButtonForThemeBeingEdited.Tag, dontSetIfContains: new[] { App.Text.Active, App.Text.Editing });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            for (var j = 0; j < wpInstalledThemes.Children.Count; j++)
            {
                var sp = wpInstalledThemes.Children[j];
                if (sp == ((Button)sender).Tag)
                {
                    //Remove it off this planet
                    string themeName = ((TextBlock)((StackPanel)((StackPanel)wpInstalledThemes.Children[j]).Children[0]).Children[0]).Text;
                    if (themeName.IndexOf("(") != -1)
                        themeName = themeName.Remove(themeName.IndexOf("("));
                    wpInstalledThemes.Children.Remove((StackPanel)sp);
                    ThemeNames.Remove(themeName.Trim());
                    if (!string.IsNullOrWhiteSpace(ThemeNameThatBeingEdited))
                    {
                        ThemeThatBeingEditedIntLocation = j;
                    }

                    //Delete it (tony stark i don't feel so good....)
                    File.Delete(((Frame)((StackPanel)sp).Children[1]).Tag.ToString());

                    //Remove the StackPanel that was containing it if it's lonely
                    if (wpInstalledThemes.Children.Count == 0)
                    {
                        wpInstalledThemes.Children.Remove(wpInstalledThemes);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private async void Theme_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.Config.ActiveTheme = ((Frame) sender).Tag.ToString();
            App.Config.Save();
            SetThemeStatusInUI("", removeActiveOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Active, themesFrame: ((Frame) sender));
            UpdateGlobalUI();
        }

        private void Colour_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedBorder != null)
                SelectedBorder.SetResourceReference(Border.BorderBrushProperty, "AccentColour4SCBrush");

            SelectedBorder = (Border) sender;
            SelectedBorder.SetResourceReference(Border.BorderBrushProperty, "AccentColour5SCBrush");

            colourPicker.IsEnabled = true;
            colourPicker.SelectedColor = ((SolidColorBrush) ((Border)sender).Background).Color;
        }

        private void ColourPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!IsInitialized)
                return;

            string solidColorBrushName = null;
            string colorName = null;

            //Update the border with the colour the person wants
            switch (SelectedBorder.Name)
            {
                case "borderColour1":
                    ThemeDictionary["AccentColour1SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour1"] = e.NewValue.Value;
                    borderColour1.Background = (SolidColorBrush) ThemeDictionary["AccentColour1SCBrush"];
                    solidColorBrushName = "AccentColour1SCBrush";
                    colorName = "AccentColour1";
                    break;
                case "borderColour2":
                    ThemeDictionary["AccentColour2SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour2"] = e.NewValue.Value;
                    borderColour2.Background = (SolidColorBrush)ThemeDictionary["AccentColour2SCBrush"];
                    solidColorBrushName = "AccentColour2SCBrush";
                    colorName = "AccentColour2";
                    break;
                case "borderColour2Hover":
                    ThemeDictionary["AccentColour2HoverSCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour2Hover"] = e.NewValue.Value;
                    borderColour2Hover.Background = (SolidColorBrush)ThemeDictionary["AccentColour2HoverSCBrush"];
                    solidColorBrushName = "AccentColour2HoverSCBrush";
                    colorName = "AccentColour2Hover";
                    break;
                case "borderColour3":
                    ThemeDictionary["AccentColour3SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour3"] = e.NewValue.Value;
                    borderColour3.Background = (SolidColorBrush)ThemeDictionary["AccentColour3SCBrush"];
                    solidColorBrushName = "AccentColour3SCBrush";
                    colorName = "AccentColour3";
                    break;
                case "borderColour4":
                    ThemeDictionary["AccentColour4SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour4"] = e.NewValue.Value;
                    borderColour4.Background = (SolidColorBrush)ThemeDictionary["AccentColour4SCBrush"];
                    solidColorBrushName = "AccentColour4SCBrush";
                    colorName = "AccentColour4";
                    break;
                case "borderColour5":
                    ThemeDictionary["AccentColour5SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour5"] = e.NewValue.Value;
                    borderColour5.Background = (SolidColorBrush)ThemeDictionary["AccentColour5SCBrush"];
                    solidColorBrushName = "AccentColour5SCBrush";
                    colorName = "AccentColour5";
                    break;
                case "borderTextColour":
                    ThemeDictionary["TextColourSCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["TextColour"] = e.NewValue.Value;
                    borderTextColour.Background = (SolidColorBrush)ThemeDictionary["TextColourSCBrush"];
                    solidColorBrushName = "TextColourSCBrush";
                    colorName = "TextColour";
                    break;
                case "borderDiscordButtonColour":
                    ThemeDictionary["DisabledButtonColour"] = new SolidColorBrush(e.NewValue.Value);
                    borderDiscordButtonColour.Background = (SolidColorBrush)ThemeDictionary["DisabledButtonColour"];
                    solidColorBrushName = "DisabledButtonColour";
                    break;
                case "borderDiscordButtonTextColour":
                    ThemeDictionary["DisabledButtonTextColour"] = new SolidColorBrush(e.NewValue.Value);
                    borderDiscordButtonTextColour.Background = (SolidColorBrush)ThemeDictionary["DisabledButtonTextColour"];
                    solidColorBrushName = "DisabledButtonTextColour";
                    break;
                case "borderSelectedPageColour":
                    ThemeDictionary["NavButtonBackgroundSelected"] = new SolidColorBrush(e.NewValue.Value);
                    borderSelectedPageColour.Background = (SolidColorBrush)ThemeDictionary["NavButtonBackgroundSelected"];
                    solidColorBrushName = "NavButtonBackgroundSelected";
                    break;
                case "borderSelectedPageIconColour":
                    ThemeDictionary["NavButtonIconColourSelected"] = new SolidColorBrush(e.NewValue.Value);
                    borderSelectedPageIconColour.Background = (SolidColorBrush)ThemeDictionary["NavButtonIconColourSelected"];
                    solidColorBrushName = "NavButtonIconColourSelected";
                    break;
            }

            //Update the test UI with the colour
            if (frameThemeBeingMade.Content != null)
                ((MainPageThumbnail)frameThemeBeingMade.Content).UpdateMergedDictionaries(solidColorBrushName, new SolidColorBrush(e.NewValue.Value), colorName);
            else
                frameThemeBeingMade.Content = new MainPageThumbnail(ThemeDictionary);
        }

        private async void BtnSaveAndApplyTheme_OnClick(object sender, RoutedEventArgs e)
        {
            var frame = await SaveTheme();

            //Click the frame programmatically
            MouseButtonEventArgs doubleClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Left);
            doubleClickEvent.RoutedEvent = Control.MouseDoubleClickEvent;
            doubleClickEvent.Source = this;
            frame?.RaiseEvent(doubleClickEvent);
        }

        private async void BtnSaveTheme_OnClick(object sender, RoutedEventArgs e)
        {
            SaveTheme();
        }

        private void TbCurrentThemeName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //Check the theme name
            if (tbCurrentThemeName.Text.Length == 0)
            {
                tbCurrentThemeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbCurrentThemeName.ToolTip = new ToolTip($"{App.Text.ThemeNeedName}!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                btnSaveTheme.IsEnabled = false;
                return;
            }

            string newThemeName = tbCurrentThemeName.Text.Trim();
            bool sameThemeName = true;
            if (ThemeNameThatBeingEdited != null)
                sameThemeName = !ThemeNameThatBeingEdited.Equals(newThemeName);

            if (ThemeNames.Contains(newThemeName) && sameThemeName)
            {
                tbCurrentThemeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbCurrentThemeName.ToolTip = new ToolTip($"{App.Text.ThemeWithSameName}!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                btnSaveTheme.IsEnabled = false;
                return;
            }

            for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
            {
                if (newThemeName.Contains(Path.GetInvalidFileNameChars()[i]))
                {
                    tbCurrentThemeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                    tbCurrentThemeName.ToolTip = new ToolTip($"{App.Text.InvalidThemeName}!!!");
                    btnSaveAndApplyTheme.IsEnabled = false;
                    btnSaveTheme.IsEnabled = false;
                    return;
                }
            }

            tbCurrentThemeName.ToolTip = null;
            tbCurrentThemeName.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
            btnSaveAndApplyTheme.IsEnabled = true;
            btnSaveTheme.IsEnabled = true;
            ThemeDictionary["ThemeName"] = newThemeName;
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
            RemoveButtonForThemeBeingEdited = null;

            //Get theme's C O N T E N T
            SetThemeStatusInUI("", removeEditOnOtherTextBoxes: true);
            MakeThemeUIEditable();
            frameThemeBeingMade.Content = new MainPageThumbnail(ThemeDictionary);
        }

        private void ThemeEditorPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            wpInstalledThemes.MaxWidth = e.NewSize.Width;
        }
    }
}
