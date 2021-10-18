using Avalonia.Controls;

namespace MultiRPC.UI.Pages
{
    public abstract class SidePage : UserControl
    {
        public abstract string IconLocation { get; }

        public abstract string LocalizableName { get; }

        public void Initialize()
        {
            if (!IsInitialized)
            {
                Initialize(true);
            }
        }

        public abstract void Initialize(bool loadXaml);
    }
}