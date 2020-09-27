using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static MultiRPC.Core.LanguagePicker;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MultiRPC.Shared.UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomPage : LocalizablePage, ISidePage, IRpcPage
    {
        public CustomPage()
        {
            this.InitializeComponent();
            Loaded += (sender, _) => Accessed?.Invoke(sender, null);
        }

        public event EventHandler Accessed;

        public override void UpdateText()
        {
            tblID.Text = $"{GetLineFromLanguageFile("ClientID")}:";
            tblText1.Text = $"{GetLineFromLanguageFile("Text1")}:";
            tblText2.Text = $"{GetLineFromLanguageFile("Text2")}:";
            tblLargeKey.Text = $"{GetLineFromLanguageFile("LargeKey")}:";
            tblLargeText.Text = $"{GetLineFromLanguageFile("LargeText")}:";
            tblSmallKey.Text = $"{GetLineFromLanguageFile("SmallKey")}:";
            tblSmallText.Text = $"{GetLineFromLanguageFile("SmallText")}:";
            tblElapasedTime.Text = $"{GetLineFromLanguageFile("ShowElapsedTime")}:";
        }

        public string IconLocation => "Icon/Page/Custom";

        public string LocalizableName => "Custom";

        public RichPresence RichPresence => throw new NotImplementedException();
    }
}
