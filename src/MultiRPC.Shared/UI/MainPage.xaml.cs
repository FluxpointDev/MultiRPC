using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core;
using static MultiRPC.Core.LanguagePicker;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : LocalizablePage
    {
        readonly List<ToolTip> ToolTips = new();
        Button activateButton;

        public MainPage()
        {
            InitializeComponent();

            topbar.Content = new TopBar();

            //UWP/WinUI Loading
            Loading += OnLoading;

            //Wasm Loading
            //Loading += OnLoading;
        }

        private async void OnLoading(FrameworkElement sender, object args)
        //private async void OnLoading(object sender, RoutedEventArgs e)
        {
            //Get all the side pages 
            var pages = ServiceManager.ServiceProvider.GetServices<ISidePage>();
            foreach (var page in pages)
            {
                //Make the button + ToolTip for button
                var icon = await AssetManager.GetAsset(page.IconLocation, 56, 56);
                var btn = new Button
                {
                    Content = new Image
                    {
                        Source = icon as ImageSource,
                        Margin = new Thickness(3)
                    },
                    Style = (Style)this.Resources[$"{(activateButton == null ? "Active" : "")}PageButton"],
                    Name = $"btn{page.LocalizableName}"
                };
                btn.Click += Btn_Click;

                ToolTip toolTip = new()
                {
                    Content = await GetTooltipString(page)
                };
                toolTip.Tag = page;
                ToolTipService.SetToolTip(btn, toolTip);
                ToolTips.Add(toolTip);

                btn.Tag = page;
                stpSideBar.Children.Add(btn);

                if (activateButton == null)
                {
                    frmContent.Content = page;
                    activateButton = btn;
                }
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Make it get the active logo
            if (activateButton != null)
            {
                activateButton.Style = (Style)this.Resources["PageButton"];
            }

            activateButton = (Button)sender;
            activateButton.Style = (Style)this.Resources["ActivePageButton"];
            frmContent.Content = activateButton.Tag;
        }

        public override async void UpdateText()
        {
            foreach (var tooltip in ToolTips)
            {
                tooltip.Content = await GetTooltipString(tooltip.Tag as ISidePage);
            }
        }

        private static async Task<string> GetTooltipString(ISidePage page) => 
            $"{await GetLineFromLanguageFile(page.LocalizableName) ?? @"¯\_(ツ)_/¯"} {await GetLineFromLanguageFile("Page")}";
    }
}