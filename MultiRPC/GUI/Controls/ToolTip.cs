using System.Threading.Tasks;
using System.Windows;

namespace MultiRPC.GUI.Controls
{
    /// <summary>
    ///     Edited Tooltip to have a string parameter, nothing special
    /// </summary>
    public class ToolTip : System.Windows.Controls.ToolTip
    {
        /// <summary>
        ///     Tooltip with string parameter
        /// </summary>
        /// <param name="text">Text to show on the tooltip</param>
        public ToolTip(string text)
        {
            Content = text;
            UISetup();
        }

        public ToolTip()
        {
            UISetup();
        }

        private Task UISetup()
        {
            BorderThickness = new Thickness(1);
            SetResourceReference(BorderBrushProperty, "AccentColour1SCBrush");
            SetResourceReference(BackgroundProperty, "AccentColour2SCBrush");

            return Task.CompletedTask;
        }
    }
}