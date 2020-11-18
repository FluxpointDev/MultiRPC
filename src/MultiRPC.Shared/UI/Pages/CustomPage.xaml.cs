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
using static MultiRPC.Core.LanguagePicker;
using System.ComponentModel;
#if UNO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
#endif

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
        public event PropertyChangedEventHandler PropertyChanged;

        public override async void UpdateText()
        {
            tblID.Text = $"{await GetLineFromLanguageFile("ClientID")}:";
            tblText1.Text = $"{await GetLineFromLanguageFile("Text1")}:";
            tblText2.Text = $"{await GetLineFromLanguageFile("Text2")}:";
            tblLargeKey.Text = $"{await GetLineFromLanguageFile("LargeKey")}:";
            tblLargeText.Text = $"{await GetLineFromLanguageFile("LargeText")}:";
            tblSmallKey.Text = $"{await GetLineFromLanguageFile("SmallKey")}:";
            tblSmallText.Text = $"{await GetLineFromLanguageFile("SmallText")}:";
            tblElapasedTime.Text = $"{await GetLineFromLanguageFile("ShowElapsedTime")}:";
        }

        public string IconLocation => "Icon/Page/Custom";

        public string LocalizableName => "Custom";

        public RichPresence RichPresence => throw new NotImplementedException();

        public bool AllowStartingRPC => throw new NotImplementedException();
    }
}
