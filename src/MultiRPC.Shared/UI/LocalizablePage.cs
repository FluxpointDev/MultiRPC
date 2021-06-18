using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiRPC.Core.Extensions;
using Serilog;

namespace MultiRPC.Shared.UI
{
    public abstract class LocalizablePage : Page
    {
        public LocalizablePage() : base()
        {
            Loaded += OnLoading;

            //TODO: Make it trigger on lang change
        }

        //private void OnLoading(FrameworkElement sender, object e)
        private void OnLoading(object sender, RoutedEventArgs args)
        {
            try
            {
                UpdateText();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e);

            }
        }

        public abstract void UpdateText();
    }
}
