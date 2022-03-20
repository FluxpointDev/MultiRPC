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
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI.Controls;

public partial class TabsPage : Grid, ITabPage
{
    private readonly List<ITabPage> _pages = new List<ITabPage>();
    private Rectangle? _activePageRectangle;

    private static readonly Language NaLang = LanguageText.NA;
    private static DisableSettings DisableSettings { get; } = SettingManager<DisableSettings>.Setting;
    private static readonly PointerPressedEventArgs PointerArg = new PointerPressedEventArgs(null!, null!, null!,
        new Point(), 0, PointerPointProperties.None, KeyModifiers.None);

    public ITabPage this[int index] => _pages[index];
    public ITabPage this[Index index] => _pages[index];

    public Language? TabName { get; init; }
    public bool IsDefaultPage { get; init; }
    public void Initialize() => Initialize(true);
    
    public void AddTabs(IEnumerable<ITabPage> pages) => _pages.AddRange(pages);
    public void AddTabs(ITabPage[] pages) => _pages.AddRange(pages);
    public void AddTab(ITabPage page) => _pages.Add(page);
    
    public virtual void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        DisableSettings.PropertyChanged += (sender, args) => UpdateBackground();
        App.Current.GetResourceObservable("ThemeAccentColor2").Subscribe(_ => UpdateBackground());
        UpdateBackground();

        stpTabs.Children.AddRange(_pages.Select(MakeTab));

        //This grabs the default page if any and triggers the pointer event so it loads up with it
        var defaultPage = _pages.FirstOrDefault(x => x.IsDefaultPage) ?? _pages.FirstOrDefault();
        var defaultControl = stpTabs.Children.FirstOrDefault(x => x.DataContext == defaultPage);
        defaultControl?.RaiseEvent(PointerArg);
    }

    private void UpdateBackground() => content.Background =
        new ImmutableSolidColorBrush((Color)App.Current.Resources["ThemeAccentColor2"], DisableSettings.AcrylicEffect ? 1 : 0.7);

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