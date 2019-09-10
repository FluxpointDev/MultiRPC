using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for InstalledThemes.xaml
    /// </summary>
    public partial class InstalledThemes : Page
    {
        public static InstalledThemes _InstalledThemes;
        private readonly string _designerXamlFile = Path.Combine("Assets", "Themes", "DesignerTheme.xaml");

        public readonly List<string> ThemeNames = new List<string>();

        //For checking previewing the theme everywhere
        private string _lastFrameMouseWasIn;

        //This is for when someone is editing a theme, we need to know their remove button to remove and add the theme and
        //We need the theme name to allow the theme name TextBox to not disable when that is inputted while editing that theme
        public Button RemoveButtonForThemeBeingEdited;

        public string ThemeNameThatBeingEdited;

        public InstalledThemes()
        {
            InitializeComponent();
            _InstalledThemes = this;

            UpdateText(App.Text.Editing, App.Text.Active);
            var files = Directory.EnumerateFiles(Path.Combine("Assets", "Themes"))
                .Concat(Directory.EnumerateFiles(FileLocations.ThemesFolder)).ToArray();
            for (var i = 0; i < files.LongCount(); i++)
            {
                if (files[i] == _designerXamlFile)
                {
                    continue;
                }

                MakeThemeUI(files[i], putActive: true).ConfigureAwait(false);
            }
        }

        public Task UpdateText(string oldEditingWord, string oldActiveWord)
        {
            btnAddTheme.Content = App.Text.AddTheme;
            btnAddAndApplyTheme.Content = App.Text.AddAndApplyTheme;

            //Go in each themeStackPanel and update the buttons text            
#pragma warning disable 4014
            EditInstalledTheme(stackPanel =>
#pragma warning restore 4014
            {
                var themeName = (TextBlock) ((StackPanel) stackPanel.Children[0]).Children[0];
                var text = GetThemeNameFromTextBlock(themeName, oldEditingWord, oldActiveWord);

                if (text == Application.Current.Resources["ThemeName"].ToString())
                {
                    text += $" ({App.Text.Active})";
                }
                else if (!string.IsNullOrWhiteSpace(ThemeNameThatBeingEdited) && text == ThemeNameThatBeingEdited)
                {
                    text += $" ({App.Text.Editing})";
                }

                themeName.Text = text;

                var themeTopControls = (StackPanel) stackPanel.Children[0];
                for (var i = 0; i < themeTopControls.Children.Count; i++)
                {
                    if (!(themeTopControls.Children[i] is Button button))
                    {
                        continue;
                    }

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

        public async Task<Frame> MakeThemeUI(string themeFile, int themeLocation = -1, bool putActive = false)
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

            var isActiveTheme = theme.Resources["ThemeName"].ToString() ==
                                Application.Current.Resources["ThemeName"].ToString();
            var isEnabled = !themeFile.StartsWith(Path.Combine("Assets", "Themes"));
            var tblThemeName = new TextBlock
            {
                Text = theme.Resources["ThemeName"] +
                       (putActive && isActiveTheme
                           ? $" ({App.Text.Active})"
                           : ""),
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.FromOpenTypeWeight(2),
                FontSize = 16,
                MaxWidth = isEnabled ? 131 : 135,
                TextWrapping = TextWrapping.Wrap
            };
            ThemeNames.Add(theme.Resources["ThemeName"].ToString());

            //Make buttons and see if it allowed to be edited (if made by us then that would be a big fat nope)
            var removeButton = new Button
            {
                Content = App.Text.Remove,
                Name = "btnRemove",
                IsEnabled = isEnabled && !isActiveTheme,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = themeStackPanel
            };
            var editButton = new Button
            {
                Content = App.Text.Edit,
                Name = "btnEdit",
                Margin = new Thickness(5, 0, 5, 0),
                IsEnabled = isEnabled,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = new Tuple<Button, string>(removeButton, themeFile)
            };
            var cloneButton = new Button
            {
                Content = App.Text.Clone,
                Name = "btnClone",
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = themeFile
            };
            removeButton.Click += RemoveButton_Click;
            editButton.Click += EditButton_Click;
            cloneButton.Click += CloneButton_Click;

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
            if (themeLocation == -1)
            {
                wpInstalledThemes.Children.Add(themeStackPanel);
            }
            else
            {
                wpInstalledThemes.Children.Insert(themeLocation, themeStackPanel);
            }

            return frame;
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
                    {
                        themeName = themeName.Remove(themeName.IndexOf("("));
                    }

                    wpInstalledThemes.Children.Remove((StackPanel) sp);
                    ThemeNames.Remove(themeName.Trim());
                    if (!string.IsNullOrWhiteSpace(ThemeNameThatBeingEdited))
                    {
                        ThemeEditorPage._ThemeEditorPage._themeThatBeingEditedIntLocation = j;
                    }

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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //Get content in edit button
            var editContent = (Tuple<Button, string>) ((Button) sender).Tag;
            ThemeEditorPage._ThemeEditorPage.btnSaveTheme.IsEnabled =
                Application.Current.Resources["ThemeName"].ToString() !=
                Path.GetFileNameWithoutExtension(editContent.Item2);

            //Remake ThemeDictionary, set the remove button and show it at the top (always helpful)
            RemoveButtonForThemeBeingEdited = editContent.Item1;
            ThemeEditorPage._ThemeEditorPage.MakeThemeUIEditable(editContent.Item2);

            SetThemeStatusInUI("", doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing},
                removeEditOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Editing, (StackPanel) RemoveButtonForThemeBeingEdited.Tag,
                doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing});
        }

        private async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveButtonForThemeBeingEdited = null;

            //Get theme's C O N T E N T
            var themeContent = await Theme.Load(((Button) sender).Tag.ToString());

            //Edit name to have copy so user's know it's the cloned one
            if (!themeContent.ThemeMetadata.ThemeName.Contains("("))
            {
                themeContent.ThemeMetadata.ThemeName += " (1)";
            }

            var tmpName = themeContent.ThemeMetadata.ThemeName;
            var copyCount = 1;
            while (ThemeNames.Contains(tmpName))
            {
                tmpName = tmpName.Replace($"({copyCount})", "") + $"({copyCount + 1})";
                copyCount++;
            }

            themeContent.ThemeMetadata.ThemeName = tmpName;

            Theme.Save(themeContent);

            //Show it on the screen (that's always helpful)
            await MakeThemeUI(Theme.GetThemeFileLocation(themeContent));
        }

        public async Task SetThemeStatusInUI(string status, StackPanel themesStackPanel = null,
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
                    {
                        return;
                    }

                    text += $" ({status})";
                    text = text.Replace(" ()", "");
                    themeName.Text = text;

                    var themeFile = ((Frame) stackPanel.Children[1]).Tag.ToString();
                    ((Button) ((StackPanel) stackPanel.Children[0]).Children[2]).IsEnabled =
                        !themeFile.StartsWith(Path.Combine("Assets", "Themes")) && App.Config.ActiveTheme != themeFile;
                }
                else if (removeActiveOnOtherTextBoxes &&
                         themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Active)
                {
                    themeName.Text = text;

                    var themeFile = ((Frame) stackPanel.Children[1]).Tag.ToString();
                    ((Button) ((StackPanel) stackPanel.Children[0]).Children[2]).IsEnabled =
                        !themeFile.StartsWith(Path.Combine("Assets", "Themes"));
                }
                else if (removeEditOnOtherTextBoxes &&
                         themeName.Text.Split(' ').Last().Replace("(", "").Replace(")", "") == App.Text.Editing)
                {
                    themeName.Text = text;
                }
            });
        }

        private void Frame_MouseLeave(object sender, MouseEventArgs e)
        {
            _lastFrameMouseWasIn = null;
            ThemeEditorPage.UpdateGlobalUI();
            SetThemeStatusInUI("", themesFrame: (Frame) sender,
                doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing});
        }

        private async void Frame_MouseEnter(object sender, MouseEventArgs e)
        {
            var lastFrameMouseWasIn = _lastFrameMouseWasIn = (string) ((Frame) sender).Tag;
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            if (lastFrameMouseWasIn == _lastFrameMouseWasIn)
            {
                ThemeEditorPage.UpdateGlobalUI(lastFrameMouseWasIn);
                SetThemeStatusInUI(App.Text.Showing, themesFrame: (Frame) sender,
                    doNotSetIfContains: new[] {App.Text.Active, App.Text.Editing});
            }
        }

        private void Theme_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.Config.ActiveTheme = ((Frame) sender).Tag.ToString();
            App.Config.Save();
            SetThemeStatusInUI("", removeActiveOnOtherTextBoxes: true);
            SetThemeStatusInUI(App.Text.Active, themesFrame: (Frame) sender);
            ThemeEditorPage.UpdateGlobalUI();
        }

        private Task EditInstalledTheme(Action<StackPanel> action)
        {
            for (var j = 0; j < wpInstalledThemes.Children.Count; j++)
            {
                action((StackPanel) wpInstalledThemes.Children[j]);
            }

            return Task.CompletedTask;
        }

        private string GetThemeNameFromTextBlock(TextBlock textBlock, string editingWord = null,
            string activeWord = null, string showingWord = null)
        {
            if (string.IsNullOrWhiteSpace(editingWord))
            {
                editingWord = App.Text.Editing;
            }

            if (string.IsNullOrWhiteSpace(activeWord))
            {
                activeWord = App.Text.Active;
            }

            if (string.IsNullOrWhiteSpace(showingWord))
            {
                showingWord = App.Text.Showing;
            }

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
                if (tmp != textBlock.Text.Length)
                {
                    text = text.Remove(tmp);
                }
            }
            else if (bracketIndex != -1 && (lastBracketContent == editingWord ||
                                            lastBracketContent == activeWord ||
                                            lastBracketContent == showingWord))
            {
                text = text.Remove(bracketIndex).Trim();
            }

            return text;
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
                {
                    File.Move(openFile.FileNames[i],
                        Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));
                }

                var themeFiles = new List<string>();
                for (var i = 0; i < openFile.SafeFileNames.LongLength; i++)
                {
                    themeFiles.Add(Path.Combine(FileLocations.ThemesFolder, openFile.SafeFileNames[i]));
                }

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
                for (var i = 0; i < themeFiles.Length; i++)
                {
                    await MakeThemeUI(themeFiles[i]);
                }
            }
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