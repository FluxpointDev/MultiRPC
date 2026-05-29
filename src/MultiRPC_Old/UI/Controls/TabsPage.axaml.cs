using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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
        var defaultControl = stpTabs.Children.OfType<StackPanel>().FirstOrDefault(x => x.DataContext == defaultPage);
        if (defaultControl != null && defaultPage != null)
        {
            ShowTab(defaultPage, (Rectangle)defaultControl.Children[1]);
        }
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

        stc.PointerExited += (sender, args) =>
        {
            if (!Equals(rec, _activePageRectangle))
            {
                rec.Height = 0;
            }
        };
        stc.PointerEntered += (sender, args) =>
        {
            if (!Equals(rec, _activePageRectangle))
            {
                rec.Height = 1;
            }
        };
        stc.PointerPressed += (sender, args) => ShowTab(page, rec);
        return stc;
    }

    private void ShowTab(ITabPage page, Rectangle rec)
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
    }
}

public interface ITabPage : IControl
{
    public Language? TabName { get; }

    public bool IsDefaultPage { get; }

    public void Initialize(bool loadXaml);
}