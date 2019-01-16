using MultiRPC.GUI;
using System.Windows;
using System.Windows.Controls;

namespace MultiRPC.Functions
{
    public static class FuncSettings
    {
        public static void ToggleSetting(MainWindow window, string name)
        {
            switch (name)
            {
                case "TokenCheck":
                    App.Log.App("Toggled token check");
                    break;
                case "Afk":
                    App.Log.App("Toggled afk time");
                    App.Config.AFKTime = window.ToggleAfkTime.IsChecked.Value;
                    break;
                case "ProgramsTab":
                    App.Log.App("Toggled programs tab");
                    App.Config.Disabled.ProgramsTab = window.ToggleProgramsTab.IsChecked.Value;
                    SetProgramsTab(window);
                    break;
                case "HelpIcons":
                    App.Log.App("Toggled help icons");
                    App.Config.Disabled.HelpIcons = window.ToggleHelpIcons.IsChecked.Value;
                    if (Views.Custom != null)
                    {
                        if (window.ToggleHelpIcons.IsChecked.Value)
                            Views.Custom.DisableHelpIcons();
                        else
                            Views.Custom.EnableHelpIcons();
                    }
                    break;
                default:
                    App.Log.Error("App", "Unknown setting toggled");
                    break;
            }
            App.Config.Save();
        }

        public static void SelectAutoStart(ComboBox comboBox)
        {
            App.Config.AutoStart = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            App.Config.Save();
        }

        public static void SetProgramsTab(MainWindow window)
        {
            if (window.ToggleProgramsTab.IsChecked.Value)
            {
                window.TabPrograms.Width = 0;
                window.TabPrograms.Visibility = Visibility.Hidden;
            }
            else
            {
                window.TabPrograms.Width = 67;
                window.TabPrograms.Visibility = Visibility.Visible;
            }
        }
    }
}
