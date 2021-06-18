using System;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using static MultiRPC.Core.LanguagePicker;
using MultiRPC.Shared.UI.Views;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MultiRPC.Core.Pages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MultiRPCPage : LocalizablePage, ISidePage, IRpcPage
    {
        private RichPresence _richPresence = new RichPresence("MultiRPC", Constants.MultiRPCID);
        public RichPresence RichPresence => _richPresence;

        public event EventHandler Accessed;
        public event PropertyChangedEventHandler PropertyChanged;

        public MultiRPCPage()
        {
            this.InitializeComponent();
            cclEditor.Content = new PresenceEditorView(ref _richPresence, false);
            Loaded += (sender, _) =>
            {
                Accessed?.Invoke(sender, null);
            };
        }

        public override void UpdateText()
        {
            tblWhatWillLookLike.Text = GetLineFromLanguageFile("WhatItWillLookLike");
        }

        public string IconLocation => "Icon/Page/Discord";

        public string LocalizableName => "MultiRPC";

        public bool VaildRichPresence => _richPresence.IsVaildPresence;
    }
}
