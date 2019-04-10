using System;
using System.IO;
using System.Linq;
using System.Windows;
using MultiRPC.GUI.Views;
using System.Windows.Input;
using System.Windows.Media;
using MultiRPC.JsonClasses;
using System.Windows.Markup;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        private Border SelectedBorder = null;

        public ThemeEditorPage()
        {
            InitializeComponent();

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
        }

        private Task MakeThemeUIThatGoingToBeEdited()
        {
            Color getColor(Brush brush)
            {
                return ((SolidColorBrush) brush).Color;
            }

            var darkTheme = (ResourceDictionary) XamlReader.Parse(File.ReadAllText("Assets/Themes/DarkTheme.xaml"));
            frameThemeBeingMade.Content = new MainPageThumbnail(darkTheme);

            borderColour1.Background = ((SolidColorBrush)darkTheme["AccentColour1SCBrush"]);
            borderColour2.Background = ((SolidColorBrush)darkTheme["AccentColour2SCBrush"]);
            borderColour2Hover.Background = ((SolidColorBrush)darkTheme["AccentColour2HoverSCBrush"]);
            borderColour3.Background = ((SolidColorBrush)darkTheme["AccentColour3SCBrush"]);
            borderColour4.Background = ((SolidColorBrush)darkTheme["AccentColour4SCBrush"]);
            borderColour5.Background = ((SolidColorBrush)darkTheme["AccentColour5SCBrush"]);
            borderTextColour.Background = ((SolidColorBrush)darkTheme["TextColourSCBrush"]);

            borderDiscordButtonColour.Background = ((SolidColorBrush)darkTheme["DisabledButtonColour"]);
            borderDiscordButtonTextColour.Background = ((SolidColorBrush)darkTheme["DisabledButtonTextColour"]);
            borderSelectedPageColour.Background = ((SolidColorBrush)darkTheme["NavButtonBackgroundSelected"]);

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

            ThemeDictionary.Add("AccentColour1", getColor(borderColour1.Background));
            ThemeDictionary.Add("AccentColour2", getColor(borderColour2.Background));
            ThemeDictionary.Add("AccentColour2Hover", getColor(borderColour2Hover.Background));
            ThemeDictionary.Add("AccentColour3", getColor(borderColour3.Background));
            ThemeDictionary.Add("AccentColour4", getColor(borderColour4.Background));
            ThemeDictionary.Add("AccentColour5", getColor(borderColour5.Background));
            ThemeDictionary.Add("TextColour", getColor(borderTextColour.Background));

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

            StackPanel nameEditAndRemoveStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0,0,0,5)
            };

            TextBlock tblThemeName = new TextBlock
            {
                Text = (string)theme.Resources["ThemeName"],
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.FromOpenTypeWeight(2),
                FontSize = 18
            };
            Button editButton = new Button
            {
                Content = "Edit Theme",
                Margin = new Thickness(5,0,5,0)
            };
            Button removeButton = new Button
            {
                Content = "Remove Theme"
            };
            removeButton.Click += RemoveButton_Click;

            //MAKE ADD THEME BUTTON AND DRAG AND DROP

            nameEditAndRemoveStackPanel.Children.Add(tblThemeName);
            nameEditAndRemoveStackPanel.Children.Add(editButton);
            nameEditAndRemoveStackPanel.Children.Add(removeButton);

            themeStackPanel.Children.Add(nameEditAndRemoveStackPanel);
            themeStackPanel.Children.Add(frame);
            stackPanelToAddTo.Children.Add(themeStackPanel);
            removeButton.Tag = themeStackPanel;

            return frame;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel staclPanelToEdit = null;
            foreach (var child in spInstalledThemes.Children)
            {
                if (child is StackPanel stackPanel)
                {
                    foreach (var sp in stackPanel.Children)
                    {
                        if (sp == ((Button)sender).Tag)
                        {
                            stackPanel.Children.Remove((StackPanel)sp);
                            File.Delete(((Frame)((StackPanel)sp).Children[1]).Tag.ToString());

                            if (stackPanel.Children.Count == 0)
                            {
                                spInstalledThemes.Children.Remove(stackPanel);
                            }
                            return;
                        }
                    }
                }
            }
        }

        public static async Task UpdateGlobalUI()
        {
            while (MainPage.mainPage.frameRPCPreview.Content == null)
                await Task.Delay(250);

            App.Current.Resources.MergedDictionaries.Clear();
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText(App.Config.ActiveTheme)));
            App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));

            var frameRPCPreviewBG = MainPage.mainPage.frameRPCPreview.Content != null ? ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).gridBackground.Background : ((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
            if (((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]).Color && ((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Red"]).Color && ((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Purple"]).Color)
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

        private async Task<Frame> SaveTheme()
        {
            var themeFile = Path.Combine(FileLocations.ThemesFolder, tbCurrentThemeName.Text + ".multirpctheme");
            var stack = ((StackPanel)spInstalledThemes.Children[spInstalledThemes.Children.Count - 1]);
            if (stack.Children.Count == 2)
            {
                stack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                }; spInstalledThemes.Children.Add(stack);
            }

            File.WriteAllText(themeFile, XamlWriter.Save(ThemeDictionary));
            var frame = await MakeThemeUI(stack.Children.Count == 1, themeFile, stack);
            ThemeDictionary = new ResourceDictionary();
            tbCurrentThemeName.Clear();
            await MakeThemeUIThatGoingToBeEdited();
            return frame;
        }

        private async void BtnSaveTheme_OnClick(object sender, RoutedEventArgs e)
        {
            SaveTheme();
        }

        private void TbCurrentThemeName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCurrentThemeName.Text.Length == 0)
            {
                btnSaveAndApplyTheme.IsEnabled = false;
                btnSaveTheme.IsEnabled = false;
                return;
            }

            for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
            {
                if (tbCurrentThemeName.Text.Contains(Path.GetInvalidFileNameChars()[i]))
                {
                    tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                    tbCurrentThemeName.ToolTip = new ToolTip("This is a invalid name!!!");
                    btnSaveAndApplyTheme.IsEnabled = false;
                    btnSaveTheme.IsEnabled = false;
                    return;
                }
                tbCurrentThemeName.ToolTip = null;
                tbCurrentThemeName.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                btnSaveAndApplyTheme.IsEnabled = true;
                btnSaveTheme.IsEnabled = true;
            }

            ThemeDictionary["ThemeName"] = tbCurrentThemeName.Text;
        }
    }
}
