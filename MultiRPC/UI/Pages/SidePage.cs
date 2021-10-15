using Avalonia.Controls;

namespace MultiRPC.UI.Pages
{
    public abstract class SidePage : UserControl
    {
        public abstract string IconLocation { get; }

        public abstract string LocalizableName { get; }
    }
}