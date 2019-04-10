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
        ResourceDictionary ThemeDictionary = new ResourceDictionary();
        List<string> ThemeNames = new List<string>();
        private Border SelectedBorder = null;
        private Button RemoveButtonForThemeBeingEdited;
        private string ThemeNameThatBeingEdited;

        private static ThemeEditorPage themeEditor;

        public ThemeEditorPage()
        {
            InitializeComponent();
            themeEditor = this;

            MakeThemeUIThatGoingToBeEdited();

            StackPanel stackPanelToAddTo = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            bool onSecondTheme = false;
            foreach (var themeFile in Directory.EnumerateFiles(Path.Combine("Assets", "Themes")).Concat(Directory.EnumerateFiles(FileLocations.ThemesFolder)))
            {
                MakeThemeUI(onSecondTheme, themeFile, stackPanelToAddTo).ConfigureAwait(false).GetAwaiter().GetResult();

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

            if (stackPanelToAddTo.Children.Count > 0)
                spInstalledThemes.Children.Add(stackPanelToAddTo);

            UpdateText();

            MouseButtonEventArgs doubleClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Left);
            doubleClickEvent.RoutedEvent = Control.MouseDownEvent;
            doubleClickEvent.Source = this;
            borderColour1.RaiseEvent(doubleClickEvent);
        }

        private void ThemeEditorPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbCurrentThemeName.Text))
                tbCurrentThemeName.Text = (string)ThemeDictionary["ThemeName"] + " " + ThemeNames.Count;

            UpdateText();
        }

        public async Task UpdateText()
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

            btnSaveTheme.Content = App.Text.SaveTheme;
            btnSaveAndApplyTheme.Content = App.Text.SaveAndApplyTheme;
            tblInstalledThemes.Text = App.Text.InstalledThemes;

            foreach (var content in spInstalledThemes.Children)
            {
                if (!(content is StackPanel stackPanel))
                    return;

                foreach (StackPanel themeStackpanel in stackPanel.Children)
                {
                    StackPanel themeTopControls = (StackPanel)themeStackpanel.Children[0];
                    foreach (var control in themeTopControls.Children)
                    {
                        if (control is Button button)
                        {
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
                    }
                }
            }
        }

        private async Task AddThemeButtonLogic(bool apply)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "MultiRPC Theme | *.multirpctheme";
            openFile.Title = "Add Theme";
            openFile.Multiselect = !apply; //This is because we wouldn't know what theme to apply
            if (openFile.ShowDialog().Value)
            {
                for (int i = 0; i < openFile.FileNames.Length; i++)
                {
                    File.Move(openFile.FileNames[i], Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));
                }

                List<string> themeFiles = new List<string>();
                foreach (var ThemefileName in openFile.SafeFileNames)
                {
                    themeFiles.Add(Path.Combine(FileLocations.ThemesFolder, ThemefileName));
                }
                await AddExternalTheme(apply, themeFiles.ToArray());
            }
        }

        private async Task<StackPanel> GetThemeStackpanel()
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

            return stack;
        }

        public async Task AddExternalTheme(bool apply, string[] themeFiles)
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
            ThemeNameThatBeingEdited = null;
            RemoveButtonForThemeBeingEdited?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var themeFile = Path.Combine(FileLocations.ThemesFolder, tbCurrentThemeName.Text + ".multirpctheme");
            var stack = await GetThemeStackpanel();

            File.WriteAllText(themeFile, XamlWriter.Save(ThemeDictionary));
            var frame = await MakeThemeUI(stack.Children.Count == 1, themeFile, stack);
            ThemeDictionary = new ResourceDictionary();
            tbCurrentThemeName.Clear();
            await MakeThemeUIThatGoingToBeEdited();
            RemoveButtonForThemeBeingEdited = null;
            return frame;
        }

        public static async Task UpdateGlobalUI()
        {
            while (MainPage.mainPage.frameRPCPreview.Content == null)
                await Task.Delay(250);

            App.Current.Resources.MergedDictionaries.Clear();
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText(App.Config.ActiveTheme)));
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));

            var frameRPCPreviewBG = MainPage.mainPage.frameRPCPreview.Content != null ? ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).gridBackground.Background : ((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
            if (((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Red"]).Color && ((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Purple"]).Color)
            {
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateBackground(
                    (SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateForground(
                    (SolidColorBrush)App.Current.Resources["TextColourSCBrush"]);
            }
            MainPage.mainPage.RerenderButtons();

            ((MainWindow)App.Current.MainWindow).TaskbarIcon.TrayToolTip = new ToolTip(App.Current.MainWindow.WindowState == WindowState.Minimized ? App.Text.ShowMultiRPC : App.Text.HideMultiRPC);
            var preview = ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content);
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

            if (CustomPage.customPage.CurrentButton != null)
                CustomPage.customPage.CurrentButton.Background = (SolidColorBrush)App.Current.Resources["AccentColour2HoverSCBrush"];

            string tmp = themeEditor.tbCurrentThemeName.Text;
            themeEditor.tbCurrentThemeName.Text = "";
            themeEditor.tbCurrentThemeName.Text = tmp;
            themeEditor.tblMakeTheme.ToolTip = new ToolTip($"{App.Text.ShareThemePart1}\r\n{FileLocations.ThemesFolder} {App.Text.ShareThemePart2}!");
        }

        private Task MakeThemeUIThatGoingToBeEdited(string themeFile = "Assets/Themes/DarkTheme.xaml")
        {
            Color getColor(Brush brush)
            {
                return ((SolidColorBrush) brush).Color;
            }

            var theme = (ResourceDictionary) XamlReader.Parse(File.ReadAllText(themeFile));
            frameThemeBeingMade.Content = new MainPageThumbnail(theme);

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

            ThemeDictionary.Add("AccentColour1SCBrush", borderColour1.Background);
            ThemeDictionary.Add("AccentColour2SCBrush", borderColour2.Background);
            ThemeDictionary.Add("AccentColour2HoverSCBrush", borderColour2Hover.Background);
            ThemeDictionary.Add("AccentColour3SCBrush", borderColour3.Background);
            ThemeDictionary.Add("AccentColour4SCBrush", borderColour4.Background);
            ThemeDictionary.Add("AccentColour5SCBrush", borderColour5.Background);
            ThemeDictionary.Add("TextColourSCBrush", borderTextColour.Background);
            ThemeDictionary.Add("DisabledButtonColour", borderDiscordButtonColour.Background);
            ThemeDictionary.Add("DisabledButtonTextColour", borderDiscordButtonTextColour.Background);
            ThemeDictionary.Add("NavButtonBackgroundSelected", borderSelectedPageColour.Background);
            ThemeDictionary.Add("NavButtonIconColourSelected", borderSelectedPageIconColour.Background);
            ThemeDictionary.Add("AccentColour1", getColor(borderColour1.Background));
            ThemeDictionary.Add("AccentColour2", getColor(borderColour2.Background));
            ThemeDictionary.Add("AccentColour2Hover", getColor(borderColour2Hover.Background));
            ThemeDictionary.Add("AccentColour3", getColor(borderColour3.Background));
            ThemeDictionary.Add("AccentColour4", getColor(borderColour4.Background));
            ThemeDictionary.Add("AccentColour5", getColor(borderColour5.Background));
            ThemeDictionary.Add("TextColour", getColor(borderTextColour.Background));
            ThemeDictionary.Add("ThemeName", theme["ThemeName"]);

            tbCurrentThemeName.Text = "";
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

            return Task.CompletedTask;
        }

        public async Task<Frame> MakeThemeUI(bool onSecondTheme, string themeFile, StackPanel stackPanelToAddTo)
        {
            StackPanel themeStackPanel = new StackPanel();
            var frame = new Frame
            {
                Margin = !onSecondTheme ? new Thickness(0, 0, 5, 0) : new Thickness(0)
            };

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

            TextBlock tblThemeName = new TextBlock
            {
                Text = (string)theme.Resources["ThemeName"],
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.FromOpenTypeWeight(2),
                FontSize = 16
            };
            if (!ThemeNames.Contains(tblThemeName.Text))
                ThemeNames.Add(tblThemeName.Text);

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

            StackPanel nameEditAndRemoveStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };
            nameEditAndRemoveStackPanel.Children.Add(tblThemeName);
            nameEditAndRemoveStackPanel.Children.Add(editButton);
            nameEditAndRemoveStackPanel.Children.Add(removeButton);
            nameEditAndRemoveStackPanel.Children.Add(cloneButton);

            themeStackPanel.Children.Add(nameEditAndRemoveStackPanel);
            themeStackPanel.Children.Add(frame);
            stackPanelToAddTo.Children.Add(themeStackPanel);

            return frame;
        }

        private async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveButtonForThemeBeingEdited = null;

            ResourceDictionary ThemeDictionary = (ResourceDictionary)XamlReader.Parse(File.ReadAllText(((Button)sender).Tag.ToString()));
            ThemeDictionary["ThemeName"] = ThemeDictionary["ThemeName"] + " Copy";
            string themeName = Path.Combine(FileLocations.ThemesFolder, ThemeDictionary["ThemeName"].ToString());
            File.WriteAllText(themeName + ".multirpctheme",XamlWriter.Save(ThemeDictionary));

            var stack = await GetThemeStackpanel();
            await MakeThemeUI(stack.Children.Count == 1, themeName + ".multirpctheme", stack);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Tuple<Button, string> editContent = (Tuple<Button, string>) ((Button) sender).Tag;
            ThemeDictionary = new ResourceDictionary();
            RemoveButtonForThemeBeingEdited = editContent.Item1;
            MakeThemeUIThatGoingToBeEdited(editContent.Item2);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanelToEdit = null;
            for (var i = 0; i < spInstalledThemes.Children.Count; i++)
            {
                var child = spInstalledThemes.Children[i];
                if (child is StackPanel stackPanel)
                {
                    if (stackPanelToEdit != null)
                    {
                        var theme = (StackPanel)stackPanel.Children[0];
                        stackPanel.Children.Remove(theme);
                        stackPanelToEdit.Children.Add(theme);
                        ((StackPanel)stackPanelToEdit.Children[0]).Margin = new Thickness(0,0,5,0);
                        if (stackPanelToEdit.Children.Count == 2)
                            ((StackPanel)stackPanelToEdit.Children[0]).Margin = new Thickness(0);

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
                                string themeName = ((TextBlock)((StackPanel)((StackPanel)stackPanel.Children[j]).Children[0]).Children[0]).Text;
                                stackPanel.Children.Remove((StackPanel) sp);
                                ThemeNames.Remove(themeName);

                                File.Delete(((Frame) ((StackPanel) sp).Children[1]).Tag.ToString());

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
                            if(i == spInstalledThemes.Children.Count - 1 && j == stackPanel.Children.Count - 1)
                                return;
                        }
                    }
                }
            }
        }

        private async void Theme_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.Config.ActiveTheme = ((Frame) sender).Tag.ToString();
            App.Config.Save();
            UpdateGlobalUI();
        }

        private void Colour_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedBorder != null)
                SelectedBorder.BorderBrush = (SolidColorBrush) App.Current.Resources["AccentColour4SCBrush"];

            SelectedBorder = (Border) sender;
            SelectedBorder.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour5SCBrush"];

            colourPicker.IsEnabled = true;
            colourPicker.SelectedColor = ((SolidColorBrush) SelectedBorder.Background).Color;
        }

        private void ColourPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(!IsInitialized)
                return;

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

            frameThemeBeingMade.Content = new MainPageThumbnail(ThemeDictionary);
        }

        private async void BtnSaveAndApplyTheme_OnClick(object sender, RoutedEventArgs e)
        {
            var frame = await SaveTheme();
            MouseButtonEventArgs doubleClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Left);
            doubleClickEvent.RoutedEvent = Control.MouseDoubleClickEvent;
            doubleClickEvent.Source = this;
            frame?.RaiseEvent(doubleClickEvent);
        }

        private async void BtnSaveTheme_OnClick(object sender, RoutedEventArgs e)
        {
            await SaveTheme();
        }

        private void TbCurrentThemeName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCurrentThemeName.Text.Length == 0)
            {
                tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbCurrentThemeName.ToolTip = new ToolTip("The profile needs a name!!!");
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
                tbCurrentThemeName.ToolTip = new ToolTip("There is a profile already called that!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                btnSaveTheme.IsEnabled = false;
                return;
            }

            for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
            {
                if (newThemeName.Contains(Path.GetInvalidFileNameChars()[i]))
                {
                    tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                    tbCurrentThemeName.ToolTip = new ToolTip("This is a invalid name!!!");
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
    }
}
