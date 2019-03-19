using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MultiRPC.JsonClasses
{
    public class Theme
    {
        public enum ActiveTheme
        {
            Dark = 0,
            Light = 1,
            Custom = 2
        }

        public static Theme LightTheme = new Theme
        {
            AccentColour1 = Color.FromRgb(255,255,255),
            AccentColour2 = Color.FromRgb(244, 247, 248),
            AccentColour2Hover = Color.FromRgb(239, 239, 239),
            AccentColour3 = Color.FromRgb(209, 209, 209),
            AccentColour4 = Color.FromRgb(180, 180, 180),
            AccentColour5 = Color.FromRgb(112, 112, 112),
            TextColour = Color.FromRgb(112, 112, 112)
        };

        public static Theme DarkTheme = new Theme
        {
            AccentColour1 = Color.FromRgb(54, 57, 62),
            AccentColour2 = Color.FromRgb(44, 46, 48),
            AccentColour2Hover = Color.FromRgb(44, 42, 42),
            AccentColour3 = Color.FromRgb(255, 255, 255),
            AccentColour4 = Color.FromRgb(180, 180, 180),
            AccentColour5 = Color.FromRgb(112, 112, 112),
            TextColour = Color.FromRgb(255, 255, 255)
        };


        public Color AccentColour1;
        public Color AccentColour2;
        public Color AccentColour2Hover;
        public Color AccentColour3;
        public Color AccentColour4;
        public Color AccentColour5;
        public Color TextColour;
    }
}
