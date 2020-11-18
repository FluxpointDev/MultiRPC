using System;
#if WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#endif

namespace MultiRPC.Shared.UI
{
    public abstract class LocalizablePage : Page
    {
        public LocalizablePage() : base()
        {
            //UWP/WINUI Loading
            Loading += OnLoading;

            //WASM Loading
            //Loading += OnLoading;

            //TODO: Make it trigger on lang change
        }

        //private void OnLoading(object sender, RoutedEventArgs e) =>
        private void OnLoading(FrameworkElement sender, object args) =>
            UpdateText();

        public virtual void UpdateText() => throw new NotImplementedException();
    }
}
