using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace MultiRPC.UI.Controls
{
    public sealed partial class TabsPage : UserControl
    {
        private readonly List<ITabPage> _pages = new List<ITabPage>();
        public void AddTabs(params ITabPage[] pages) => _pages.AddRange(pages);
        
        public void Initialize()
        {
            InitializeComponent();
            stpTabs.Children.AddRange(_pages.Select(MakeTab));

            //This grabs the default page if any and triggers the pointer event so it loads up with it
            var defaultPage = _pages.FirstOrDefault(x => x.IsDefaultPage);
            var defaultControl = stpTabs.Children.FirstOrDefault(x => x.DataContext == defaultPage);
            defaultControl?.RaiseEvent(
                new PointerPressedEventArgs(null!, null!, null!,
                    new Point(), 0, PointerPointProperties.None, KeyModifiers.None));
        }

        private static Language naLang = new Language("N/A");
        private Rectangle? _activePageRectangle;
        private Control MakeTab(ITabPage page)
        {
            var text = new TextBlock
            {
                [!TextBlock.TextProperty] = (page.TabName?.TextObservable ?? naLang.TextObservable).ToBinding(),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var rec = new Rectangle
            {
                Height = 0,
                Fill = Brushes.White
            };

            var stc = new StackPanel
            {
                Orientation = Orientation.Vertical,
                DataContext = page,
                Children =
                {
                    text,
                    rec
                }
            };

            stc.PointerLeave += (sender, args) =>
            {
                if (!Equals(rec, _activePageRectangle))
                {
                    rec.Height = 0;
                }
            };
            stc.PointerEnter += (sender, args) =>
            {
                if (!Equals(rec, _activePageRectangle))
                {
                    rec.Height = 1;
                }
            };
            stc.PointerPressed += (sender, args) =>
            {
                if (_activePageRectangle is not null)
                {
                    _activePageRectangle.Height = 0;
                }

                if (!page.IsInitialized)
                {
                    page.Initialize(true);
                }

                _activePageRectangle = rec;
                _activePageRectangle.Height = 3;
                content.Content = page;
            };
            return stc;
        }
    }

    public interface ITabPage
    {
        public Language? TabName { get; }

        public bool IsDefaultPage { get; }
        public bool IsInitialized { get; }

        public void Initialize(bool loadXaml);
    }
}