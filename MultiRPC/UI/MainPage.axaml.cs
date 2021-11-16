using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Svg;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Pages;
using ShimSkiaSharp;

namespace MultiRPC.UI
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            
            Button? btnToTrigger = null;
            ISidePage? pageToTrigger = null;
            var autoStartPageName = SettingManager<GeneralSettings>.Setting.AutoStart;
            foreach (var page in PageManager.CurrentPages)
            {
                var btn = AddSidePage(page);
                btnToTrigger ??= btn;
                pageToTrigger ??= page;
                if (page.LocalizableName == autoStartPageName)
                {
                    btnToTrigger = btn;
                    pageToTrigger = page;
                }
            }
            PageManager.PageAdded += (sender, page) => AddSidePage(page);
            SideButton_Clicked(btnToTrigger, null!, pageToTrigger!);

            //If auto start has been selected then we want load that up if possible
            if (pageToTrigger?.LocalizableName == autoStartPageName)
            {
                _autoStartPage = ((RpcPage)pageToTrigger);
                if (_autoStartPage.PresenceValid)
                {
                    TriggerStart();
                    return;
                }

                _ = WaitForValidPresence();
            }
        }

        private async Task WaitForValidPresence()
        {
            _autoStartPage.PresenceValidChanged += OnPresenceValidChanged;
            await Task.Delay(1024 * 5);
            _autoStartPage.PresenceValidChanged -= OnPresenceValidChanged;
        }
        
        private readonly RpcPage _autoStartPage;
        private void OnPresenceValidChanged(object? sender, bool e)
        {
            if (e)
            {
                TriggerStart();
                _autoStartPage.PresenceValidChanged -= OnPresenceValidChanged;
            }
        }

        private void TriggerStart()
        {
            if (_autoStartPage == cclContent.Content)
            {
                topbar.TriggerStartStop();
            }
        }
        
        private Button? _selectedBtn;
        private Button AddSidePage(ISidePage page)
        {
            var source = SvgSource.Load("avares://MultiRPC/Assets/" + page.IconLocation + ".svg", null);
            UpdateIconColour(source, (Color)Application.Current.Resources["ThemeAccentColor3"]!);
            var btn = new Button
            {
                Content = new Image
                {
                    Margin = new Thickness(4.5),
                    Source = new SvgImage
                    {
                        Source = source
                    }
                },
                Tag = source
            };

            Application.Current.GetResourceObservable("ThemeAccentColor3").Subscribe(x =>
            {
                if (_selectedBtn != btn)
                {
                    UpdateButtonIconColour(btn, (Color)x!);
                }
            });
            Application.Current.GetResourceObservable("NavButtonSelectedIconColor").Subscribe(x =>
            {
                if (_selectedBtn == btn)
                {
                    UpdateButtonIconColour(btn, (Color)x!);
                }
            });

            var lang = new Language(page.LocalizableName);
            lang.TextObservable.Subscribe(s => ToolTip.SetTip(btn, s));

            btn.Click += (sender, args) => SideButton_Clicked(sender, args, page);
            btn.Classes.Add("nav");
            splPages.Children.Add(btn);
            return btn;
        }

        private void UpdateButtonIconColour(ContentControl btn, Color color)
        {
            var source = (SvgSource)btn.Tag!;
            UpdateIconColour(source, color);
            ((Image)btn.Content).Source = new SvgImage
            {
                Source = source
            };
        }

        private void UpdateIconColour(SvgSource source, Color color)
        {
            foreach (var commands in source.Picture?.Commands ?? ArraySegment<CanvasCommand>.Empty)
            {
                if (commands is DrawPathCanvasCommand pathCanvasCommand)
                {
                    pathCanvasCommand.Paint.Shader = SKShader.CreateColor(new SKColor(color.R, color.G, color.B, color.A), SKColorSpace.Srgb);
                }
            }
        }

        private void SideButton_Clicked(object? sender, RoutedEventArgs e, ISidePage page)
        {
            if (sender is not Button btn
            || btn == _selectedBtn)
            {
                return;
            }

            if (_selectedBtn != null)
            {
                UpdateButtonIconColour(_selectedBtn, (Color)Application.Current.Resources["ThemeAccentColor3"]!);
                _selectedBtn.Classes.Remove("selected");
            }
            btn.Classes.Insert(0,"selected");
            UpdateButtonIconColour(btn, (Color)Application.Current.Resources["NavButtonSelectedIconColor"]!);

            _selectedBtn = btn;
            if (!page.IsInitialized)
            {
                page.Initialize();
            }

            contentBorder.Background = page.BackgroundColour.HasValue 
                ? new SolidColorBrush(page.BackgroundColour.Value) 
                : (IBrush)Application.Current.Resources["ThemeAccentBrush2"]!;
            cclContent.Padding = page.ContentPadding;
            cclContent.Content = page;
            
            if (page is RpcPage rpcPage)
            {
                RpcPageManager.PageMoved(rpcPage);
            }
        }
    }
}