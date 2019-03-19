using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MultiRPC.GUI.Controls
{
    /// <summary>
    /// Edited Tooltip to have a string parameter, nothing special
    /// </summary>
    public class ToolTip : System.Windows.Controls.ToolTip
    {
        /// <summary>
        /// Tooltip with string parameter
        /// </summary>
        /// <param name="Text">Text to show on the tooltip</param>
        public ToolTip(string Text)
        {
            Content = Text;
            UISetup();
        }

        public async Task UISetup()
        {
            BorderThickness = new Thickness(1);
            BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour1SCBrush"];
            Background = (SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"];
        }

        public ToolTip()
        {
            UISetup();
        }
    }
}
