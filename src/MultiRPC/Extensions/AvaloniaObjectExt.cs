using Avalonia;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace MultiRPC.Extensions;

public static class AvaloniaObjectExt
{
    public static void RunUILogic(this AvaloniaObject avaloniaObject, Action action)
    {
        if (!avaloniaObject.CheckAccess())
        {
            Dispatcher.UIThread.Post(action);
            return;
        }
            
        action.Invoke();
    }
    
    public static RenderTargetBitmap RenderToBitmap(this ILayoutable target)
    {
        var pixelSize = new PixelSize((int) target.Width, (int) target.Height);
        var size = new Size(target.Width, target.Height);
        RenderTargetBitmap bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        target.Measure(size);
        target.Arrange(new Rect(size));
        bitmap.Render(target);
        return bitmap;
    }
}