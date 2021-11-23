using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using MultiRPC.Extensions;

namespace MultiRPC.UI.Controls
{
    public class Hyperlink : TextBlock, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TextBlock);

        public Hyperlink()
        {
            TextDecorations = TextDecorationCollection.Parse("Underline");
            Cursor = new Cursor(StandardCursorType.Hand);
            PointerPressed += (sender, args) => Uri.OpenInBrowser();
        }
        
        public static readonly StyledProperty<string> UriProperty = AvaloniaProperty.Register<Hyperlink, string>(nameof(Uri));
        public string Uri
        {
            get => GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }
    }
}