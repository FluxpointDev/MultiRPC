﻿using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MultiRPC.Core.Pages;
using Windows.Foundation.Metadata;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MultiRPC.Shared.UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Experimental]
    public sealed partial class LoggingPage : LocalizablePage, ISidePage
    {
        public LoggingPage()
        {
            this.InitializeComponent();
        }

        public string IconLocation => "Icon/Page/Logging";

        public string LocalizableName => "Log";
        
        public override void UpdateText()
        {
        }
    }
}