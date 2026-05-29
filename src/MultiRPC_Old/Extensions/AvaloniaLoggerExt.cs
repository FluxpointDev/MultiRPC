using Avalonia.Controls;
using Avalonia.Logging;
using MultiRPC.Logging;

namespace MultiRPC.Extensions;

public static class AvaloniaLoggerExt
{
    /// <summary>
    /// Logs Avalonia events to the <see cref="AvaloniaLogger"/> sink.
    /// </summary>
    /// <typeparam name="T">The application class type.</typeparam>
    /// <param name="builder">The app builder instance.</param>
    /// <param name="areas">The areas to log. Valid values are listed in <see cref="LogArea"/>.</param>
    /// <returns>The app builder instance.</returns>
    public static T LogToTinyUpdate<T>(
        this T builder,
        params string[] areas)
        where T : AppBuilderBase<T>, new()
    {
        Logger.Sink = new AvaloniaLogger(areas);
        return builder;
    }
}