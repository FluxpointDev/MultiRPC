using System.ComponentModel;
using Avalonia.Controls;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI;

/// <summary>
/// This helps us use our system for disabling tooltips when they are not wanted
/// </summary>
public static class CustomToolTip
{
    static CustomToolTip()
    {
        DisableSettings.PropertyChanged += DisableSettingsOnPropertyChanged;
    }

    private static void DisableSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Setting.Settings.DisableSettings.AllTooltips))
        {
            foreach (var (key, value) in StoredTooltips)
            {
                SetTooltip(key, value);
            }
        }
    }

    private static readonly Dictionary<Control, object?> StoredTooltips = new Dictionary<Control, object?>();
    private static readonly DisableSettings DisableSettings = SettingManager<DisableSettings>.Setting;
    public static void SetTip(Control element, object? value)
    {
        if (!StoredTooltips.ContainsKey(element))
        {
            StoredTooltips.Add(element, value);
        }
        StoredTooltips[element] = value;
        SetTooltip(element, value);
    }
    
    private static void SetTooltip(Control element, object? value)
    {
        if (!DisableSettings.AllTooltips)
        {
            ToolTip.SetTip(element, value);
            return;
        }
        
        ToolTip.SetTip(element, null);
    }
}