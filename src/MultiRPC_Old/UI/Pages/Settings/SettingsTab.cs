using Avalonia.Controls;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Controls.Settings;

namespace MultiRPC.UI.Pages.Settings;

public class SettingsTab : StackPanel, ITabPage
{
    public SettingsTab()
    {
        Spacing = 7;
    }
        
    public Language? TabName { get; init; }
    public bool IsDefaultPage { get; }
    public void Initialize(bool loadXaml) { }

    public void Add(SettingItem settingItem)
    {
        Children.Add(settingItem);
    }
}