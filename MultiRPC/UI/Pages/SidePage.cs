using Avalonia.Controls;
using Avalonia.Media;

namespace MultiRPC.UI.Pages
{
    public abstract class SidePage : UserControl
    {
        public abstract string IconLocation { get; }

        public abstract string LocalizableName { get; }

        public abstract void Initialize(bool loadXaml);
        
        public void Initialize()
        {
            if (!IsInitialized)
            {
                Initialize(true);
            }
        }

        public Color? BackgroundColour { get; protected set; }
    }
}