using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Svg;
using MultiRPC.UI.Pages;

namespace MultiRPC.UI
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            
            Button? btnToTrigger = null;
            SidePage? pageToTrigger = null;
            foreach (var page in PageManager.CurrentPages)
            {
                var btn = AddSidePage(page);
                btnToTrigger ??= btn;
                pageToTrigger ??= page;
            }
            PageManager.PageAdded += (sender, page) => AddSidePage(page);

            //TODO: Add autostart
            btnToTrigger?.Classes.Add("selected");
            pageToTrigger?.Initialize();
            cclContent.Content = pageToTrigger;
            selectedBtn = btnToTrigger;
        }

        private Button? selectedBtn;
        private Button AddSidePage(SidePage page)
        {
            var btn = new Button
            {
                Content = new Image
                {
                    Margin = new Thickness(4.5),
                    Source = new SvgImage
                    {
                        Source = SvgSource.Load("avares://MultiRPC/Assets/" + page.IconLocation + ".svg", null)
                    }
                }
            };
            ToolTip.SetTip(btn, Language.GetText(page.LocalizableName) + " " + Language.GetText("Page"));

            //TODO: See why the visual doesn't update on the spot
            btn.Click += delegate
            {
                selectedBtn?.Classes.Remove("selected");
                btn.Classes.Insert(0,"selected");

                selectedBtn = btn;
                if (!page.IsInitialized)
                {
                    page.Initialize();
                }

                contentBorder.Material.TintColor = page.BackgroundColour ?? (Color)App.Current.Resources["ThemeAccentColor2"];
                cclContent.Background = page.Background;
                cclContent.Content = page;
            };

            btn.Classes.Add("nav");
            splPages.Children.Add(btn);
            return btn;
        }
    }
}