using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using MultiRPC.Shared.UI;
using MultiRPC.Core;
using MultiRPC.Core.Pages;
using Windows.Foundation.Metadata;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MultiRPC.Shared.UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Experimental]
    public sealed partial class CreditsPage : LocalizablePage, ISidePage
    {
        public CreditsPage()
        {
            this.InitializeComponent();
        }

        public string IconLocation => "Icon/Page/Credits";

        public string LocalizableName => "Credits";

        public override void UpdateText()
        {
        }
    }
}
