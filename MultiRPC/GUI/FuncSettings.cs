using System.Windows;
using System.Windows.Controls;

namespace MultiRPC.GUI
{
    public static class FuncSettings
    {
        public static void ToggleSetting(MainWindow wd, string name)
        {
            switch(name)
            {
                case "DiscordCheck":
                    RPC.Log.App("Toggled discord check");
                    RPC.Config.Disabled.DiscordCheck = !RPC.Config.Disabled.DiscordCheck;
                    break;
                case "TokenCheck":
                    RPC.Log.App("Toggled token check");
                    RPC.Config.Disabled.DiscordCheck = !RPC.Config.Disabled.DiscordCheck;
                    break;
                case "Afk":
                    RPC.Log.App("Toggled afk time");
                    RPC.Config.AFKTime = !RPC.Config.AFKTime;
                    break;
                case "ProgramsTab":
                    RPC.Log.App("Toggled programs tab");
                    RPC.Config.Disabled.DiscordCheck = !RPC.Config.Disabled.DiscordCheck;
                    if (wd.ToggleProgramsTab.IsChecked.Value)
                    {
                        wd.TabPrograms.Width = 0;
                        wd.TabPrograms.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        wd.TabPrograms.Width = 67;
                        wd.TabPrograms.Visibility = Visibility.Visible;
                    }
                    break;
                case "HelpIcons":
                    RPC.Log.App("Toggled help icons");
                    RPC.Config.Disabled.DiscordCheck = !RPC.Config.Disabled.DiscordCheck;
                    if (wd.ToggleHelpIcons.IsChecked.Value)
                        DisableHelpIcons();
                    else
                        EnableHelpIcons();
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

        public static void EnableHelpIcons()
        {
            App.WD.HelpClientID.Visibility = Visibility.Visible;
            App.WD.HelpText1.Visibility = Visibility.Visible;
            App.WD.HelpText2.Visibility = Visibility.Visible;
            App.WD.HelpLargeKey.Visibility = Visibility.Visible;
            App.WD.HelpLargeText.Visibility = Visibility.Visible;
            App.WD.HelpSmallKey.Visibility = Visibility.Visible;
            App.WD.HelpSmallText.Visibility = Visibility.Visible;
        }

        public static void DisableHelpIcons()
        {
            App.WD.HelpClientID.Visibility = Visibility.Hidden;
            App.WD.HelpText1.Visibility = Visibility.Hidden;
            App.WD.HelpText2.Visibility = Visibility.Hidden;
            App.WD.HelpLargeKey.Visibility = Visibility.Hidden;
            App.WD.HelpLargeText.Visibility = Visibility.Hidden;
            App.WD.HelpSmallKey.Visibility = Visibility.Hidden;
            App.WD.HelpSmallText.Visibility = Visibility.Hidden;
        }
    }
}
