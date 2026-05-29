using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using MultiRPC.Extensions;

namespace MultiRPC.UI.Controls;

public class Hyperlink : TextBlock, IStyleable
{
    Type IStyleable.StyleKey => typeof(TextBlock);

    private static readonly TextDecorationCollection _texDec = TextDecorationCollection.Parse("Underline");
    private static readonly Cursor _cursor = new Cursor(StandardCursorType.Hand);
    public Hyperlink()
    {
        TextDecorations = _texDec;
        Cursor = _cursor;
        PointerPressed += (sender, args) => Uri.OpenInBrowser();
    }
        
    public static readonly StyledProperty<string> UriProperty = AvaloniaProperty.Register<Hyperlink, string>(nameof(Uri));
    public string Uri
    {
        get => GetValue(UriProperty);
        set => SetValue(UriProperty, value);
    }
}