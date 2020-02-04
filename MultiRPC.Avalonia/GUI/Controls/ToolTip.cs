using Avalonia;
using System.Threading.Tasks;
using System.Windows;

namespace MultiRPC.Avalonia.GUI.Controls
{
    /// <summary>
    /// Edited Tooltip to have a string parameter, nothing special
    /// </summary>
    public class ToolTip : global::Avalonia.Controls.ToolTip
    {
        /// <inheritdoc cref="Avalonia.Controls.ToolTip"/>
        /// <param name="text">Text to show on the tooltip</param>
        public ToolTip(string text)
        {
            Content = text;
            UISetup();
        }

        /// <inheritdoc cref="System.Windows.Controls.ToolTip"/>
        public ToolTip()
        {
            UISetup();
        }

        private Task UISetup()
        {
            BorderThickness = new Thickness(1);
            //SetResourceReference(BorderBrushProperty, "AccentColour1SCBrush");
            //SetResourceReference(BackgroundProperty, "AccentColour2SCBrush");

            return Task.CompletedTask;
        }
    }
}