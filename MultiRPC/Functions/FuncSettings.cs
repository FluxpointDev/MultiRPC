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
                    RPC.Log.App("Toggled token check");
                    break;
                case "Afk":
                    RPC.Log.App("Toggled afk time");
                    RPC.Config.AFKTime = window.ToggleAfkTime.IsChecked.Value;
                    break;
                case "ProgramsTab":
                    RPC.Log.App("Toggled programs tab");
                    RPC.Config.Disabled.ProgramsTab = window.ToggleProgramsTab.IsChecked.Value;
                    SetProgramsTab(window);
                    break;
                case "HelpIcons":
                    RPC.Log.App("Toggled help icons");
                    RPC.Config.Disabled.HelpIcons = window.ToggleHelpIcons.IsChecked.Value;
                    if (MainWindow.CustomPage != null)
                    {
                        if (window.ToggleHelpIcons.IsChecked.Value)
                            MainWindow.CustomPage.DisableHelpIcons();
                        else
                            MainWindow.CustomPage.EnableHelpIcons();
                    }
                    break;
                default:
                    RPC.Log.Error("App", "Unknown setting toggled");
                    break;
            }
            RPC.Config.Save();
        }

        public static void SelectAutoStart(ComboBox comboBox)
        {
            RPC.Config.AutoStart = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            RPC.Config.Save();
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
