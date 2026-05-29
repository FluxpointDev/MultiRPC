using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace MultiRPC.UI.Controls;

public partial class ColourButton : StackPanel
{
    private readonly StyledProperty<ImmutableSolidColorBrush> _btnColourProperty = AvaloniaProperty.Register<ColourButton, ImmutableSolidColorBrush>(nameof(BtnColor), (ImmutableSolidColorBrush)Brushes.White.ToImmutable());
    public ImmutableSolidColorBrush BtnColor
    {
        get => GetValue(_btnColourProperty);
        set
        {
            SetValue(_btnColourProperty, value, BindingPriority.Style);
            brdColour.Background = BtnColor;
        }
    }

    private readonly StyledProperty<Language> _languageProperty = AvaloniaProperty.Register<ColourButton, Language>(nameof(Language), LanguageText.NA);
    public Language Language
    {
        get => GetValue(_languageProperty);
        set
        {
            SetValue(_languageProperty, value);
            tblName.DataContext = Language;
        }
    }

    public ColourButton()
    {
        InitializeComponent();
        tblName.DataContext = Language;
        brdColour.Background = BtnColor;
    }
}