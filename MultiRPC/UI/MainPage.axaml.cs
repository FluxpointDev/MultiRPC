using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
            ISidePage? pageToTrigger = null;
            foreach (var page in PageManager.CurrentPages)
            {
                var btn = AddSidePage(page);
                btnToTrigger ??= btn;
                pageToTrigger ??= page;
            }
            PageManager.PageAdded += (sender, page) => AddSidePage(page);

            //TODO: Add autostart
            ClickBtn(btnToTrigger, null!, pageToTrigger!);
        }

        private Button? _selectedBtn;
        private Button AddSidePage(ISidePage page)
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
            btn.Click += (sender, args) => ClickBtn(sender, args, page);

            btn.Classes.Add("nav");
            splPages.Children.Add(btn);
            return btn;
        }

        private void ClickBtn(object? sender, RoutedEventArgs e, ISidePage page)
        {
            if (sender is not Button btn)
            {
                return;
            }
            
            _selectedBtn?.Classes.Remove("selected");
            btn.Classes.Insert(0,"selected");

            _selectedBtn = btn;
            if (!page.IsInitialized)
            {
                page.Initialize();
            }

            contentBorder.Background = page.BackgroundColour != null 
                ? new SolidColorBrush(page.BackgroundColour.Value) 
                : (IBrush)Application.Current.Resources["ThemeAccentBrush2"]!;
            cclContent.Padding = page.ContentPadding;
            cclContent.Content = page;
        }
    }
}