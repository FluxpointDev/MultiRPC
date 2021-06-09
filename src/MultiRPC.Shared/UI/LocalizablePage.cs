using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;

namespace MultiRPC.Shared.UI
{
    public abstract class LocalizablePage : Page
    {
        public LocalizablePage() : base()
        {
            //UWP/WINUI Loading
            Loading += OnLoading;

            //TODO: Make it trigger on lang change
        }

        private void OnLoading(object sender, RoutedEventArgs args)
        {
            try
            {
                UpdateText();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);

            }
        }

        public abstract void UpdateText();
    }
}
