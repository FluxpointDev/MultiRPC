using MultiRPC.GUI;
using System.Windows;
using System.Windows.Controls;

namespace MultiRPC.Functions
{
    public static class FuncSettings
    {
        public static void ToggleSetting(MainWindow window, string name)
        {
            switch(name)
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
                    if (window.ToggleHelpIcons.IsChecked.Value)
                        DisableHelpIcons(window);
                    else
                        EnableHelpIcons(window);
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

        public static void EnableHelpIcons(MainWindow window)
        {
            window.HelpClientID.Visibility = Visibility.Visible;
            window.HelpText1.Visibility = Visibility.Visible;
            window.HelpText2.Visibility = Visibility.Visible;
            window.HelpLargeKey.Visibility = Visibility.Visible;
            window.HelpLargeText.Visibility = Visibility.Visible;
            window.HelpSmallKey.Visibility = Visibility.Visible;
            window.HelpSmallText.Visibility = Visibility.Visible;
        }

        public static void DisableHelpIcons(MainWindow window)
        {
            window.HelpClientID.Visibility = Visibility.Hidden;
            window.HelpText1.Visibility = Visibility.Hidden;
            window.HelpText2.Visibility = Visibility.Hidden;
            window.HelpLargeKey.Visibility = Visibility.Hidden;
            window.HelpLargeText.Visibility = Visibility.Hidden;
            window.HelpSmallKey.Visibility = Visibility.Hidden;
            window.HelpSmallText.Visibility = Visibility.Hidden;
        }
    }
}
