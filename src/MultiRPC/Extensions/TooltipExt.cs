using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Extensions;

public static class TooltipExt
{
    public static void UpdateTooltip(this IEnumerable<Control> controls, string? text)
    {
        text = string.IsNullOrWhiteSpace(text) ? null : text;
        foreach (var control in controls)
        {
            CustomToolTip.SetTip(control, text);
        }
    }
    
    public static void UpdateTooltip(this Control control, string? text)
    {
        text = string.IsNullOrWhiteSpace(text) ? null : text;
        CustomToolTip.SetTip(control, text);
    }
}