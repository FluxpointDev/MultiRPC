using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages.Rpc.Custom.Popups
{
    public partial class SharePage : UserControl
    {
        public SharePage()
        {
            if (!Design.IsDesignMode)
            {
                throw new Exception("Shouldn't be calling this when not in designer!");
            }
        }

        private RichPresence _activeRichPresence;
        public SharePage(RichPresence activeRichPresence)
        {
            _activeRichPresence = activeRichPresence;
            InitializeComponent();
        }
    }
}