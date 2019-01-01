using MultiRPC.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Functions
{
    public static class FuncData
    {
        public static void MainData(MainWindow window)
        {
            if (RPC.Config.Disabled.ProgramsTab)
                window.TabPrograms.Width = 0;
            if (RPC.Config.MultiRPC != null)
            {
                window.TextDefaultText1.Text = RPC.Config.MultiRPC.Text1;
                window.TextDefaultText2.Text = RPC.Config.MultiRPC.Text2;
                window.TextDefaultLarge.Text = RPC.Config.MultiRPC.LargeText;
                window.TextDefaultSmall.Text = RPC.Config.MultiRPC.SmallText;
                if (RPC.Config.MultiRPC.LargeKey != -1)
                    window.ItemsDefaultLarge.SelectedItem = window.ItemsDefaultLarge.Items[RPC.Config.MultiRPC.LargeKey];
                if (RPC.Config.MultiRPC.SmallKey != -1)
                    window.ItemsDefaultSmall.SelectedItem = window.ItemsDefaultSmall.Items[RPC.Config.MultiRPC.SmallKey];
            }
            if (RPC.Config.Custom != null)
            {
                window.TextCustomClientID.Text = RPC.Config.Custom.ID.ToString();
                window.TextCustomText1.Text = RPC.Config.Custom.Text1;
                window.TextCustomText2.Text = RPC.Config.Custom.Text2;
                window.TextCustomLargeKey.Text = RPC.Config.Custom.LargeKey;
                window.TextCustomLargeText.Text = RPC.Config.Custom.LargeText;
                window.TextCustomSmallKey.Text = RPC.Config.Custom.SmallKey;
                window.TextCustomSmallText.Text = RPC.Config.Custom.SmallText;
            }
            App.SettingsLoaded = true;
            if (RPC.Config.AutoStart == "MultiRPC")
            {
                window.ItemsAutoStart.SelectedIndex = 1;
                window.Menu.SelectedIndex = 0;
                window.BtnToggleRPC_Click(null, null);
            }
            else if (RPC.Config.AutoStart == "Custom")
            {
                RPC.Type = "custom";
                window.ItemsAutoStart.SelectedIndex = 2;
                window.Menu.SelectedIndex = 1;
                window.BtnToggleRPC_Click(null, null);
            }
            if (RPC.Config.Disabled.HelpIcons)
            {
                App.WD.ToggleHelpIcons.IsChecked = true;
                FuncSettings.DisableHelpIcons(window);
            }
            if (RPC.Config.Disabled.ProgramsTab)
            {
                App.WD.ToggleProgramsTab.IsChecked = true;
                FuncSettings.SetProgramsTab(window);
            }
            if (RPC.Config.AFKTime)
                App.WD.ToggleAfkTime.IsChecked = true;
            window.TextDev.Content = App.Developer;
           
        }
    }
}
