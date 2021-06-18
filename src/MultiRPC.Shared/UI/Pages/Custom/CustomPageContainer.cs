using Microsoft.UI.Xaml.Controls;
using MultiRPC.Core.Pages;
using MultiRPC.Core.Rpc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using MultiRPC.Core;

namespace MultiRPC.Shared.UI.Pages.Custom
{
    public class CustomPageContainer : PageContainer, ISidePage
    {
        public CustomPageContainer(CustomPage customPage) 
            : base(new[] { customPage })
        {
            //TODO: Make ProfilesUI + ProfileEditingUI
            Profiles.Add(new CustomProfile() { Name = "OwO" });
            Profiles.Add(new CustomProfile() { Name = "UwU" });
            Profiles.Add(new CustomProfile() { Name = "Wow" });

            Tabs.HorizontalAlignment = HorizontalAlignment.Left;
            Tabs.Margin = new Thickness(0, 10, 0, 10);
            this.Content = new StackPanel
            {
                Children =
                {
                    //ProfilesUI,
                    //ProfileEditingUI,
                    new Grid()
                    {
                        Background = (Brush)Application.Current.Resources["Colour1"],
                        Children = { Tabs }
                    },
                    MakeCorneredFrame()
                }
            };
        }

        public StackPanel ProfilesUI { get; }

        public StackPanel ProfileEditingUI { get; } //Contains stuff to add/remove/share

        public List<CustomProfile> Profiles { get; } = new List<CustomProfile>();

        public RichPresence RichPresence { get; set; }

        public string IconLocation => "Icon/Page/Custom";

        public string LocalizableName => "Custom";
    }
}
