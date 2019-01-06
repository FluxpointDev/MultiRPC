using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiRPC.Data
{
    public class CustomProfile
    {
        public string Name = "";
        public string ClientID = "";
        public string Text1 = "";
        public string Text2 = "";
        public string LargeKey = "";
        public string LargeText = "";
        public string SmallKey = "";
        public string SmallText = "";

        public Button GetButton()
        {
            return new Button
            {
                Name = Name,
                Content = Name,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(96, 96, 96)),
                Background = new SolidColorBrush(Color.FromRgb(96, 96, 96)),
                Padding = new Thickness(10, 1, 10, 1),
                Margin = new Thickness(5, 0, 5, 0),
                Height = 20
            };
        }
    }
}
