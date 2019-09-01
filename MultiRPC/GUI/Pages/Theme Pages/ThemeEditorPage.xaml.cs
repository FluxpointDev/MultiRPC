using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MultiRPC.JsonClasses;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for ThemeEditorPage.xaml
    /// </summary>
    public partial class ThemeEditorPage : Page
    {
        public static ThemeEditorPage _ThemeEditorPage;

        //Border that has the colour we are editing
        private Border _selectedBorder;

        //ThemeDictionary for the theme being made/edited and the ThemeNames for all the theme names (so we don't need to go into 100000 StackPanel layers every time we change lol)
        private ResourceDictionary _themeDictionary = new ResourceDictionary();
        public int _themeThatBeingEditedIntLocation = -1;

        public ThemeEditorPage()
        {
            InitializeComponent();
            _ThemeEditorPage = this;

            MakeThemeUIEditable();
            UpdateText();

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
            {
                tbThemeBeingMadeName.Text = App.Text.Theme + " " +
                                            (InstalledThemes._InstalledThemes.wpInstalledThemes.Children.Count + 1);
            }
        }

        public Task UpdateText()
        {
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

            return Task.CompletedTask;
        }

        private async Task<Frame> SaveTheme()
        {
            //Null theme name because we don't need it anymore and trigger remove button to remove UI and file (if editing theme)
            InstalledThemes._InstalledThemes.RemoveButtonForThemeBeingEdited?.RaiseEvent(
                new RoutedEventArgs(ButtonBase.ClickEvent));
            InstalledThemes._InstalledThemes.ThemeNameThatBeingEdited = null;

            var themeFileContent = Theme.ResourceDictionaryToTheme(_themeDictionary);
            await Theme.Save(themeFileContent);
            var themeFile = Theme.GetThemeFileLocation(themeFileContent);

            Frame frame = null;
            frame = await InstalledThemes._InstalledThemes.MakeThemeUI(themeFile, _themeThatBeingEditedIntLocation);
            _themeThatBeingEditedIntLocation = -1;

            //Clear and reset the Theme UI that is shown when making a theme
            InstalledThemes._InstalledThemes.RemoveButtonForThemeBeingEdited = null;
            tbThemeBeingMadeName.Clear();
            await MakeThemeUIEditable();
            frmThemeBeingMade.Content = new MainPageThumbnail(_themeDictionary);
            btnSaveTheme.IsEnabled = true;

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
                if (File.Exists(App.Config.ActiveTheme))
                {
                    themeFile = App.Config.ActiveTheme;
                }
                else
                {
                    themeFile = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);
                }
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

        public async Task MakeThemeUIEditable(string themeFile = null)
        {
            if (string.IsNullOrWhiteSpace(themeFile))
            {
                themeFile = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);
            }

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
                InstalledThemes._InstalledThemes.ThemeNameThatBeingEdited = (string) themeDictionary["ThemeName"];
                tbThemeBeingMadeName.Text = InstalledThemes._InstalledThemes.ThemeNameThatBeingEdited;
            }
            else if (InstalledThemes._InstalledThemes?.ThemeNames?.Count > 0)
            {
                tbThemeBeingMadeName.Text =
                    $"{themeDictionary["ThemeName"]} " +
                    (InstalledThemes._InstalledThemes.wpInstalledThemes.Children.Count + 1);
            }

            GC.Collect();
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
            {
                return;
            }

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
            {
                ((MainPageThumbnail) frmThemeBeingMade.Content).UpdateMergedDictionaries(solidColorBrushName,
                    new SolidColorBrush(e.NewValue.Value), colorName);
            }
            else
            {
                frmThemeBeingMade.Content = new MainPageThumbnail(_themeDictionary);
            }
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
                InstalledThemes._InstalledThemes.ThemeNameThatBeingEdited
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
            var sameThemeName = false;
            if (InstalledThemes._InstalledThemes.ThemeNameThatBeingEdited != null)
            {
                sameThemeName = InstalledThemes._InstalledThemes.ThemeNameThatBeingEdited.Equals(newThemeName);
            }

            if (InstalledThemes._InstalledThemes.ThemeNames.Contains(newThemeName) && !sameThemeName)
            {
                tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbThemeBeingMadeName.ToolTip = new ToolTip($"{App.Text.ThemeWithSameName}!!!");
                btnSaveAndApplyTheme.IsEnabled = false;
                buttonToTrigger.IsEnabled = false;
                return;
            }

            for (var i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
            {
                if (newThemeName.Contains(Path.GetInvalidFileNameChars()[i]))
                {
                    tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "Red");
                    tbThemeBeingMadeName.ToolTip = new ToolTip($"{App.Text.InvalidThemeName}!!!");
                    btnSaveAndApplyTheme.IsEnabled = false;
                    buttonToTrigger.IsEnabled = false;
                    return;
                }
            }

            tbThemeBeingMadeName.ToolTip = null;
            tbThemeBeingMadeName.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
            btnSaveAndApplyTheme.IsEnabled = true;
            buttonToTrigger.IsEnabled = true;
            _themeDictionary["ThemeName"] = newThemeName;
        }

        private void BtnResetTheme_OnClick(object sender, RoutedEventArgs e)
        {
            InstalledThemes._InstalledThemes.RemoveButtonForThemeBeingEdited = null;

            //Get theme's C O N T E N T
            InstalledThemes._InstalledThemes.SetThemeStatusInUI("", removeEditOnOtherTextBoxes: true);
            MakeThemeUIEditable();
            btnSaveTheme.IsEnabled = true;
            frmThemeBeingMade.Content = new MainPageThumbnail(_themeDictionary);
        }

        private void ThemeEditorPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InstalledThemes._InstalledThemes.wpInstalledThemes.MaxWidth = e.NewSize.Width;
        }
    }
}