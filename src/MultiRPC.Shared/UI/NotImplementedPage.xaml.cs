using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
#if WINUI
using MultiRPC.WinUI3;
#endif

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NotImplementedPage : Page
    {
        int backCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromPageThatNotImplemented">If the user was on the page that is not Implemented</param>
        public NotImplementedPage(bool fromPageThatNotImplemented)
        {
            backCount = fromPageThatNotImplemented ? 2 : 1;
            this.InitializeComponent();
        }

        public void btnBack_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).GoBack(backCount);
        }
    }
}
