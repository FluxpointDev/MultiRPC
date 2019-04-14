using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using MultiRPC.GUI.Views;
using System.Windows.Input;
using System.Windows.Media;
using MultiRPC.JsonClasses;
using System.Windows.Markup;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Effects;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for ThemeEditorPage.xaml
    /// </summary>
    public partial class ThemeEditorPage : Page
    {
        public const string ThemeExtension = ".multirpctheme";

        //ThemeDictionary for the theme being made/edited and the ThemeNames for all the theme names (so we don't need to go into 100000 StackPanel layers every time we change  lol)
        private ResourceDictionary ThemeDictionary = new ResourceDictionary();
        private List<string> ThemeNames = new List<string>();

        //Border that has the colour we are editing
        private Border SelectedBorder;

        //This is for when someone is editing a theme, we need to know their remove button to remove and add the theme and
        //We need the theme name to allow the theme name TextBox to not disable when that is inputted while editing that theme
        private Button RemoveButtonForThemeBeingEdited;
        private string ThemeNameThatBeingEdited;

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

            StackPanel stackPanelToAddTo = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            bool onSecondTheme = false;
            foreach (var themeFile in Directory.EnumerateFiles(Path.Combine("Assets", "Themes")).Concat(Directory.EnumerateFiles(FileLocations.ThemesFolder)))
            {
                MakeThemeUI(onSecondTheme, themeFile, stackPanelToAddTo).ConfigureAwait(false).GetAwaiter().GetResult();

                //We want to add it to the StackPanel showing installed Themes if we got two because space is a thing ;P
                if (onSecondTheme)
                {
                    spInstalledThemes.Children.Add(stackPanelToAddTo);
                    stackPanelToAddTo = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0,10,0,0)
                    };
                }
                onSecondTheme = !onSecondTheme;
            }

            //Add any theme that hasn't been added yet
            if (stackPanelToAddTo.Children.Count > 0)
                spInstalledThemes.Children.Add(stackPanelToAddTo);

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
                tbCurrentThemeName.Text = "Theme " + ThemeNames.Count;

#pragma warning disable 4014
            EditInstalledTheme((stackpanel) =>
#pragma warning restore 4014
            {
                TextBlock themeName = (TextBlock)((StackPanel)stackpanel.Children[0]).Children[0];
                int bracketIndex = themeName.Text.IndexOf("(");
                string text;

                if (themeName.Text.StartsWith(App.Current.Resources["ThemeName"].ToString()))
                {
                    text = bracketIndex != -1
                        ? themeName.Text.Remove(bracketIndex).Trim() + $" ({App.Text.Active})"
                        : themeName.Text.Trim() + $" ({App.Text.Active})";
                }
                else if (!string.IsNullOrWhiteSpace(ThemeNameThatBeingEdited) && themeName.Text.StartsWith(ThemeNameThatBeingEdited))
                {
                    text = bracketIndex != -1
                        ? themeName.Text.Remove(bracketIndex).Trim() + $" ({App.Text.Editing})"
                        : themeName.Text.Trim() + $" ({App.Text.Editing})";
                }
                else
                {
                    text = bracketIndex != -1
                        ? themeName.Text.Remove(bracketIndex).Trim()
                        : themeName.Text.Trim();
                }
                themeName.Text = text;
            });

            //Update the UI Text in case there was a lang change
            UpdateText();
        }

        private Task UpdateText()
        {
            //No point in updating the text if there was no lang change (used this because I know colour is color in some places)
            if (tblColour1.Text == $"{App.Text.Colour1}:")
                return Task.CompletedTask;

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
                foreach (var control in themeTopControls.Children)
                {
                    if (!(control is Button button)) continue;
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
            for (int i = 0; i < spInstalledThemes.Children.Count; i++)
            {
                if (!(spInstalledThemes.Children[i] is StackPanel stackPanel)) continue;

                for (int j = 0; j < stackPanel.Children.Count; j++)
                    action((StackPanel)stackPanel.Children[j]);
            }
        }

        private async Task AddThemeButtonLogic(bool apply)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = $"MultiRPC {App.Text.Theme} | *{ThemeExtension}";
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

        private Task<StackPanel> GetThemeStackpanel()
        {
            var stack = ((StackPanel)spInstalledThemes.Children[spInstalledThemes.Children.Count - 1]);
            if (stack.Children.Count == 2)
            {
                stack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                spInstalledThemes.Children.Add(stack);
            }

            return Task.FromResult(stack);
        }

        private async Task AddExternalTheme(bool apply, string[] themeFiles)
        {
            if (apply)
            {
                var stack = await GetThemeStackpanel();
                var frame = await MakeThemeUI(stack.Children.Count == 1, themeFiles[0], stack);
                MouseButtonEventArgs doubleClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Left);
                doubleClickEvent.RoutedEvent = Control.MouseDoubleClickEvent;
                doubleClickEvent.Source = this;
                frame?.RaiseEvent(doubleClickEvent);
            }
            else
            {
                for (int i = 0; i < themeFiles.Length; i++)
                {
                    var stack = await GetThemeStackpanel();
                    await MakeThemeUI(stack.Children.Count == 1, themeFiles[i], stack);
                }
            }
        }

        private async Task<Frame> SaveTheme()
        {
            //Null theme name because we don't need it anymore and trigger remove button to remove UI and file (if editing theme)
            ThemeNameThatBeingEdited = null;
            RemoveButtonForThemeBeingEdited?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            //Get theme file name and write theme
            var themeFile = Path.Combine(FileLocations.ThemesFolder, tbCurrentThemeName.Text + ThemeExtension);
            File.WriteAllText(themeFile, XamlWriter.Save(ThemeDictionary));

            //Get StackPanel and add new theme to Installed theme's UI
            var stack = await GetThemeStackpanel();
            var frame = await MakeThemeUI(stack.Children.Count == 1, themeFile, stack);

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
            while (globalThemeBeingUpdated)
                await Task.Delay(250);

            globalThemeBeingUpdated = true;
            if (string.IsNullOrWhiteSpace(themeFile))
            {
                if (File.Exists(App.Config.ActiveTheme))
                    themeFile = App.Config.ActiveTheme;
                else
                    themeFile = "Assets/Themes/DarkTheme.xaml"; //We want to at least have a theme so this doesn't happen: https://1drv.ms/u/s!AhwsT7MDO4OvgtsoUYv7Tmq7KWDleA
            }

            //Sometimes that frame could be still not filled with C O N T E N T
            while (MainPage.mainPage.frameRPCPreview.Content == null)
                await Task.Delay(250);

            //Get rid of the old and in with the new ~~theme~~
            App.Current.Resources.MergedDictionaries.Clear();
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText(themeFile)));
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));

            //Update the RPC Frame showing the current Rich presence
            var preview = ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content);
            var frameRPCPreviewBG = MainPage.mainPage.frameRPCPreview.Content != null ? preview.gridBackground.Background : ((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
            if (((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Red"]).Color && ((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Purple"]).Color)
            {
                preview.UpdateBackground((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
                preview.UpdateForground((SolidColorBrush)App.Current.Resources["TextColourSCBrush"]);
            }

            //Rerender the main page buttons with new theme colours
            MainPage.mainPage.RerenderButtons();

            ((MainWindow)App.Current.MainWindow).TaskbarIcon.TrayToolTip = new ToolTip(App.Current.MainWindow.WindowState == WindowState.Minimized ? App.Text.ShowMultiRPC : App.Text.HideMultiRPC);

            //Update tooltips
            if (preview.ellSmallImage.ToolTip != null)
                preview.ellSmallImage.ToolTip = new ToolTip(((ToolTip)preview.ellSmallImage.ToolTip).Content.ToString());
            if (preview.recLargeImage.ToolTip != null)
                preview.recLargeImage.ToolTip = new ToolTip(((ToolTip)preview.recLargeImage.ToolTip).Content.ToString());

            preview = ((RPCPreview)MultiRPCPage.multiRpcPage.frameRPCPreview.Content);
            if (preview.ellSmallImage.ToolTip != null)
                preview.ellSmallImage.ToolTip = new ToolTip(((ToolTip)preview.ellSmallImage.ToolTip).Content.ToString());
            if (preview.recLargeImage.ToolTip != null)
                preview.recLargeImage.ToolTip = new ToolTip(((ToolTip)preview.recLargeImage.ToolTip).Content.ToString());
            SettingsPage.settingsPage.rAppDev.ToolTip = new ToolTip(((ToolTip)SettingsPage.settingsPage.rAppDev.ToolTip).Content.ToString());

            //Update the current button which tell the custom page what profile to use
            if (CustomPage.customPage.CurrentButton != null)
                CustomPage.customPage.CurrentButton.Background = (SolidColorBrush)App.Current.Resources["AccentColour2HoverSCBrush"];

            //Update the Theme name TextBox by using ~~magic~~ itself
            string tmp = themeEditor.tbCurrentThemeName.Text;
            themeEditor.tbCurrentThemeName.Clear();
            themeEditor.tbCurrentThemeName.Text = tmp;
            themeEditor.tblMakeTheme.ToolTip = new ToolTip($"{App.Text.ShareThemePart1}\r\n{FileLocations.ThemesFolder} {App.Text.ShareThemePart2}!");

            //Update the borders border (10/10 logic azy...)
            await themeEditor.UpdateBordersBorderBrush();
            if (themeEditor.SelectedBorder != null)
                themeEditor.SelectedBorder.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour5SCBrush"];
            GC.Collect();

            globalThemeBeingUpdated = false;
        }

        private Task UpdateBordersBorderBrush()
        {
            borderColour1.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderColour2.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderColour2Hover.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderColour3.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderColour4.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderColour5.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderTextColour.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderDiscordButtonColour.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderDiscordButtonTextColour.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderSelectedPageColour.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            borderSelectedPageIconColour.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
            return Task.CompletedTask;
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

        private Task MakeThemeUIEditable(string themeFile = "Assets/Themes/DarkTheme.xaml")
        {
            Color getColor(object brush)
            {
                return (Color)brush;
            }

            //Get the theme's C O N T E N T and slap it onto the screen
            var theme = (ResourceDictionary) XamlReader.Parse(File.ReadAllText(themeFile));
            frameThemeBeingMade.Content = new MainPageThumbnail(theme);

            UpdateBordersBackground(theme);
            UpdateBordersBorderBrush();

            ThemeDictionary = new ResourceDictionary();
            //Add theme's colours to ThemeDictionary
            ThemeDictionary.Add("AccentColour1SCBrush", theme["AccentColour1SCBrush"]);
            ThemeDictionary.Add("AccentColour2SCBrush", theme["AccentColour2SCBrush"]);
            ThemeDictionary.Add("AccentColour2HoverSCBrush", theme["AccentColour2HoverSCBrush"]);
            ThemeDictionary.Add("AccentColour3SCBrush", theme["AccentColour3SCBrush"]);
            ThemeDictionary.Add("AccentColour4SCBrush", theme["AccentColour4SCBrush"]);
            ThemeDictionary.Add("AccentColour5SCBrush", theme["AccentColour5SCBrush"]);
            ThemeDictionary.Add("TextColourSCBrush", theme["TextColourSCBrush"]);
            ThemeDictionary.Add("DisabledButtonColour", theme["DisabledButtonColour"]);
            ThemeDictionary.Add("DisabledButtonTextColour", theme["DisabledButtonTextColour"]);
            ThemeDictionary.Add("NavButtonBackgroundSelected", theme["NavButtonBackgroundSelected"]);
            ThemeDictionary.Add("NavButtonIconColourSelected", theme["NavButtonIconColourSelected"]);

            ThemeDictionary.Add("AccentColour1", getColor(theme["AccentColour1"]));
            ThemeDictionary.Add("AccentColour2", getColor(theme["AccentColour2"]));
            ThemeDictionary.Add("AccentColour2Hover", getColor(theme["AccentColour2Hover"]));
            ThemeDictionary.Add("AccentColour3", getColor(theme["AccentColour3"]));
            ThemeDictionary.Add("AccentColour4", getColor(theme["AccentColour4"]));
            ThemeDictionary.Add("AccentColour5", getColor(theme["AccentColour5"]));
            ThemeDictionary.Add("TextColour", getColor(theme["TextColour"]));
            ThemeDictionary.Add("ThemeName", theme["ThemeName"]);

            //Set the theme's name
            tbCurrentThemeName.Clear();
            if (themeFile != "Assets/Themes/DarkTheme.xaml")
            {
                ThemeNameThatBeingEdited = (string) theme["ThemeName"];
                tbCurrentThemeName.Text = ThemeNameThatBeingEdited;
            }
            else
            {
                if(ThemeNames.Count != 0)
                    tbCurrentThemeName.Text = (string)theme["ThemeName"] + " " + ThemeNames.Count;
            }
            GC.Collect();

            return Task.CompletedTask;
        }

        private async Task SetThemeStatusInUI(string status, StackPanel themesStackPanel = null, Frame themesFrame = null, string[] dontSetIfContains = null, bool removeActiveOnOtherTextBoxes = false, bool removeEditOnOtherTextBoxes = false)
        {
            //Go in each themeStackPanel and update the buttons text            
            await EditInstalledTheme((stackpanel) =>
            {
                if ((themesStackPanel != null && themesStackPanel == stackpanel) ||
                    (themesFrame != null && themesFrame == stackpanel.Children[1]))
                {
                    TextBlock themeName = (TextBlock)((StackPanel) stackpanel.Children[0]).Children[0];
                    if (dontSetIfContains != null && dontSetIfContains.Contains(themeName.Text.Split(' ').Last().Replace("(","").Replace(")", "")))
                        return;

                    int bracketIndex = themeName.Text.IndexOf("(");
                    string text = bracketIndex != -1
                        ? themeName.Text.Remove(bracketIndex).Trim() + $" ({status})"
                        : themeName.Text.Trim() + $" ({status})";

                    text = text.Replace(" ()", "");
                    themeName.Text = text;
                }
                else if (removeActiveOnOtherTextBoxes)
                {
                    TextBlock themeName = (TextBlock)((StackPanel)stackpanel.Children[0]).Children[0];
                    if (themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Active)
                    {
                        int bracketIndex = themeName.Text.IndexOf("(");
                        string text = bracketIndex != -1
                            ? themeName.Text.Remove(bracketIndex).Trim()
                            : themeName.Text.Trim();

                        themeName.Text = text;
                    }
                }
                else if (removeEditOnOtherTextBoxes)
                {
                    TextBlock themeName = (TextBlock)((StackPanel)stackpanel.Children[0]).Children[0];
                    if (themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Editing)
                    {
                        int bracketIndex = themeName.Text.IndexOf("(");
                        string text = bracketIndex != -1
                            ? themeName.Text.Remove(bracketIndex).Trim()
                            : themeName.Text.Trim();

                        themeName.Text = text;
                    }
                }
            });
        }

        private Task<Frame> MakeThemeUI(bool onSecondTheme, string themeFile, StackPanel stackPanelToAddTo)
        {
            //Make the StackPanel and frame the theme's UI will be in
            StackPanel themeStackPanel = new StackPanel();
            var frame = new Frame
            {
                Margin = !onSecondTheme ? new Thickness(0, 0, 5, 0) : new Thickness(0)
            };

            //Add theme to frame and make it look nice OwO
            var theme = new MainPageThumbnail((ResourceDictionary)XamlReader.Parse(File.ReadAllText(themeFile)));
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

            TextBlock tblThemeName = new TextBlock
            {
                Text = theme.Resources["ThemeName"].ToString(),
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.FromOpenTypeWeight(2),
                FontSize = 16
            };
            //Add it to the list of theme names if it somehow isn't
            if (!ThemeNames.Contains(tblThemeName.Text))
                ThemeNames.Add(tblThemeName.Text);

            //Make buttons and see if it allowed to be edited (if made by us then that would be a big fat nope)
            var isEnabled = !themeFile.StartsWith(@"Assets\Themes");
            Button editButton = new Button
            {
                Content = App.Text.Edit,
                Name = "btnEdit",
                Margin = new Thickness(5,0,5,0),
                IsEnabled = isEnabled
            };
            Button removeButton = new Button
            {
                Content = App.Text.Remove,
                Name = "btnRemove",
                IsEnabled = isEnabled
            };
            Button cloneButton = new Button
            {
                Content = App.Text.Clone,
                Name = "btnClone",
                Margin = new Thickness(5,0,0,0)
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
            stackPanelToAddTo.Children.Add(themeStackPanel);

            return Task.FromResult(frame);
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
            ResourceDictionary themeDictionary = (ResourceDictionary)XamlReader.Parse(File.ReadAllText(((Button)sender).Tag.ToString()));

            //Edit name to have copy so user's know it's the cloned one
            themeDictionary["ThemeName"] = themeDictionary["ThemeName"] + " Copy";
            string themeName = Path.Combine(FileLocations.ThemesFolder, themeDictionary["ThemeName"].ToString());
            File.WriteAllText(themeName + ".multirpctheme",XamlWriter.Save(themeDictionary));

            //Show it on the screen (that's always helpful)
            var stack = await GetThemeStackpanel();
            await MakeThemeUI(stack.Children.Count == 1, themeName + ThemeExtension, stack);
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
            //StackPanel with more StackPanel's 🙃 
            StackPanel stackPanelToEdit = null;
            for (var i = 0; i < spInstalledThemes.Children.Count; i++)
            {
                if (!(spInstalledThemes.Children[i] is StackPanel stackPanel)) continue;

                if (stackPanelToEdit != null)
                {
                    //Get theme and put it up one
                    var theme = (StackPanel)stackPanel.Children[0];
                    stackPanel.Children.Remove(theme);
                    stackPanelToEdit.Children.Add(theme);

                    //Make margin so they have their personal space
                    ((StackPanel)stackPanelToEdit.Children[0]).Margin = stackPanelToEdit.Children.Count == 2 ? new Thickness(0) : new Thickness(0, 0, 5, 0);

                    //See if we should continue with being magical
                    if (stackPanel.Children.Count != 0)
                    {
                        stackPanelToEdit = stackPanel;
                    }
                    else
                    {
                        spInstalledThemes.Children.Remove(stackPanel);
                        return;
                    }
                }
                else
                {
                    for (var j = 0; j < stackPanel.Children.Count; j++)
                    {
                        var sp = stackPanel.Children[j];
                        if (sp == ((Button) sender).Tag)
                        {
                            //Remove it off this planet
                            string themeName = ((TextBlock)((StackPanel)((StackPanel)stackPanel.Children[j]).Children[0]).Children[0]).Text;
                            stackPanel.Children.Remove((StackPanel) sp);
                            ThemeNames.Remove(themeName);

                            //Delete it (tony stark i don't feel so good....)
                            File.Delete(((Frame) ((StackPanel) sp).Children[1]).Tag.ToString());

                            //Remove the StackPanel that was containing it if it's lonely
                            if (stackPanel.Children.Count == 0)
                            {
                                spInstalledThemes.Children.Remove(stackPanel);
                                return;
                            }
                            else
                            {
                                stackPanelToEdit = stackPanel;
                            }
                        }

                        //See if we are on the last StackPanels
                        if(i == spInstalledThemes.Children.Count - 1 && j == stackPanel.Children.Count - 1)
                            return;
                    }
                }
            }
        }

        private async void Theme_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            App.Config.ActiveTheme = ((Frame) sender).Tag.ToString();
            App.Config.Save();
            UpdateGlobalUI();
            SetThemeStatusInUI("", removeActiveOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Active, themesFrame: ((Frame) sender));
        }

        private void Colour_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedBorder != null)
                SelectedBorder.BorderBrush = (SolidColorBrush) App.Current.Resources["AccentColour4SCBrush"];

            SelectedBorder = (Border) sender;
            SelectedBorder.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour5SCBrush"];

            colourPicker.IsEnabled = true;
            colourPicker.SelectedColor = ((SolidColorBrush) ((Border)sender).Background).Color;
        }

        private void ColourPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!IsInitialized)
                return;

            //Update the border with the colour the person wants
            switch (SelectedBorder.Name)
            {
                case "borderColour1":
                    ThemeDictionary["AccentColour1SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour1"] = e.NewValue.Value;
                    borderColour1.Background = (SolidColorBrush)ThemeDictionary["AccentColour1SCBrush"];
                    break;
                case "borderColour2":
                    ThemeDictionary["AccentColour2SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour2"] = e.NewValue.Value;
                    borderColour2.Background = (SolidColorBrush)ThemeDictionary["AccentColour2SCBrush"];
                    break;
                case "borderColour2Hover":
                    ThemeDictionary["AccentColour2HoverSCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour2Hover"] = e.NewValue.Value;
                    borderColour2Hover.Background = (SolidColorBrush)ThemeDictionary["AccentColour2HoverSCBrush"];
                    break;
                case "borderColour3":
                    ThemeDictionary["AccentColour3SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour3"] = e.NewValue.Value;
                    borderColour3.Background = (SolidColorBrush)ThemeDictionary["AccentColour3SCBrush"];
                    break;
                case "borderColour4":
                    ThemeDictionary["AccentColour4SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour4"] = e.NewValue.Value;
                    borderColour4.Background = (SolidColorBrush)ThemeDictionary["AccentColour4SCBrush"];
                    break;
                case "borderColour5":
                    ThemeDictionary["AccentColour5SCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["AccentColour5"] = e.NewValue.Value;
                    borderColour5.Background = (SolidColorBrush)ThemeDictionary["AccentColour5SCBrush"];
                    break;
                case "borderTextColour":
                    ThemeDictionary["TextColourSCBrush"] = new SolidColorBrush(e.NewValue.Value);
                    ThemeDictionary["TextColour"] = e.NewValue.Value;
                    borderTextColour.Background = (SolidColorBrush)ThemeDictionary["TextColourSCBrush"];
                    break;
                case "borderDiscordButtonColour":
                    ThemeDictionary["DisabledButtonColour"] = new SolidColorBrush(e.NewValue.Value);
                    borderDiscordButtonColour.Background = (SolidColorBrush)ThemeDictionary["DisabledButtonColour"];
                    break;
                case "borderDiscordButtonTextColour":
                    ThemeDictionary["DisabledButtonTextColour"] = new SolidColorBrush(e.NewValue.Value);
                    borderDiscordButtonTextColour.Background = (SolidColorBrush)ThemeDictionary["DisabledButtonTextColour"];
                    break;
                case "borderSelectedPageColour":
                    ThemeDictionary["NavButtonBackgroundSelected"] = new SolidColorBrush(e.NewValue.Value);
                    borderSelectedPageColour.Background = (SolidColorBrush)ThemeDictionary["NavButtonBackgroundSelected"];
                    break;
                case "borderSelectedPageIconColour":
                    ThemeDictionary["NavButtonIconColourSelected"] = new SolidColorBrush(e.NewValue.Value);
                    borderSelectedPageIconColour.Background = (SolidColorBrush)ThemeDictionary["NavButtonIconColourSelected"];
                    break;
            }

            //Update the test UI with the colour
            if (frameThemeBeingMade.Content != null)
                ((MainPageThumbnail)frameThemeBeingMade.Content).UpdateMergedDictionaries(ThemeDictionary);
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
                tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
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
                tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbCurrentThemeName.ToolTip = new ToolTip($"{App.Text.ThemeWithSameName}!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                btnSaveTheme.IsEnabled = false;
                return;
            }

            for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
            {
                if (newThemeName.Contains(Path.GetInvalidFileNameChars()[i]))
                {
                    tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                    tbCurrentThemeName.ToolTip = new ToolTip($"{App.Text.InvalidThemeName}!!!");
                    btnSaveAndApplyTheme.IsEnabled = false;
                    btnSaveTheme.IsEnabled = false;
                    return;
                }
            }

            tbCurrentThemeName.ToolTip = null;
            tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
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
    }
}
