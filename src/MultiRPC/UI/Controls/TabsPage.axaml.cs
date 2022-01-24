using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace MultiRPC.UI.Controls;

public sealed partial class TabsPage : UserControl
{
    private readonly List<ITabPage> _pages = new List<ITabPage>();
    public void AddTabs(IEnumerable<ITabPage> pages) => _pages.AddRange(pages);
    public void AddTabs(ITabPage[] pages) => _pages.AddRange(pages);
    public void AddTab(ITabPage page) => _pages.Add(page);

    private readonly PointerPressedEventArgs _arg = new PointerPressedEventArgs(null!, null!, null!,
        new Point(), 0, PointerPointProperties.None, KeyModifiers.None);
    public void Initialize()
    {
        InitializeComponent();
        //TODO: Make it change base on user wanting
        //content.Background = (IBrush)App.Current.Resources["ThemeAccentBrush2"];
        var colour = (Color)App.Current.Resources["ThemeAccentColor2"];
        content.Background = new ImmutableSolidColorBrush(colour, 0.7);
        App.Current.GetResourceObservable("ThemeAccentColor2").Subscribe(x =>
        {
            content.Background = new ImmutableSolidColorBrush((Color)x, 0.7);
        });
        
        stpTabs.Children.AddRange(_pages.Select(MakeTab));

        //This grabs the default page if any and triggers the pointer event so it loads up with it
        var defaultPage = _pages.FirstOrDefault(x => x.IsDefaultPage) ?? _pages.FirstOrDefault();
        var defaultControl = stpTabs.Children.FirstOrDefault(x => x.DataContext == defaultPage);
        defaultControl?.RaiseEvent(_arg);
    }

    private static readonly Language NaLang = Language.GetLanguage(LanguageText.NA);
    private Rectangle? _activePageRectangle;
    private Control MakeTab(ITabPage page)
    {
        var text = new TextBlock
        {
            [!TextBlock.TextProperty] = (page.TabName?.TextObservable ?? NaLang.TextObservable).ToBinding(),
            Margin = new Thickness(0, 0, 0, 6)
        };

        var rec = new Rectangle
        {
            Height = 0,
            [!Shape.FillProperty] = Application.Current.GetResourceObservable("ThemeForegroundBrush").ToBinding()
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