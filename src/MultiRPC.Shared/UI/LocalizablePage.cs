using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MultiRPC.Shared.UI
{
    public abstract class LocalizablePage : Page
    {
        public LocalizablePage() : base()
        {
            Loading += LocalizablePage_Loading;
        }

        private void LocalizablePage_Loading(FrameworkElement sender, object args) =>
            UpdateText();

        public virtual void UpdateText() => throw new NotImplementedException();
    }
}
