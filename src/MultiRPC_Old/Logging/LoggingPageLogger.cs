using System.Numerics;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Logging;

public class LoggingPageLogger : ILogging
{
    private static Action<TextBlock>? _action;
    private static readonly List<Lazy<TextBlock>> StoredLogging = new();

    private static readonly Language DebugLanguage = LanguageText.Debug;
    private static readonly Language InfoLanguage = LanguageText.Info;
    private static readonly Language WarnLanguage = LanguageText.Warn;
    private static readonly Language ErrorLanguage = LanguageText.Error;

    private static readonly IBrush DebugBrush = new ImmutableSolidColorBrush(Color.FromRgb(21, 230, 126));
    private static readonly IBrush InfoBrush = new ImmutableSolidColorBrush(Color.FromRgb(17, 134, 212));
    private static readonly IBrush WarnBrush = new ImmutableSolidColorBrush(Color.FromRgb(230, 125, 21));
    private static readonly IBrush ErrorBrush = new ImmutableSolidColorBrush(Colors.Red);
    private static readonly IBrush GrayBrush = new ImmutableSolidColorBrush(Color.FromRgb(209, 209, 209));

    internal static void AddAction(Action<TextBlock> action)
    {
        _action = action;
        foreach (var log in StoredLogging.TakeLast(100))
        {
            _action.Invoke(log.Value);
        }

        StoredLogging.Clear();
    }
    
    public string Name { get; }
    public LogLevel? LogLevel { get; set; }
    
    public LoggingPageLogger(string name) => Name = name;

    public void Debug(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Trace))
        {
            WriteLog(message, DebugLanguage.Text, DebugBrush, propertyValues);
        }
    }

    public void Information(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Info))
        {
            WriteLog(message, InfoLanguage.Text, InfoBrush, propertyValues);
        }
    }

    public void Warning(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Warn))
        {
            WriteLog(message, WarnLanguage.Text, WarnBrush, propertyValues);
        }
    }

    public void Error(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Error))
        {
            WriteLog(message, ErrorLanguage.Text, ErrorBrush, propertyValues);
        }
    }

    public void Error(Exception e, params object?[] propertyValues)
    {
        Error(e.Message, propertyValues);
    }

    private void WriteLog(string message, string type, IBrush colour, params object?[] propertyValues)
    {
        if (_action != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _action.Invoke(MakeRuns(message, type, colour, propertyValues));
            }, DispatcherPriority.Background);
            return;
        }
        StoredLogging.Add(new(() => MakeRuns(message, type, colour, propertyValues)));
    }

    private TextBlock MakeRuns(string message, string type, IBrush colour, params object?[] propertyValues)
    {
        var collection = AddTitle(type, colour);
        AddFormattedMessage(ref collection, message, propertyValues);
        return new TextBlock
        {
            Inlines = collection
        };
    }

    private InlineCollection AddTitle(string type, IBrush colour) =>
        new InlineCollection
        {
            new Run { FontWeight = FontWeight.Bold, Foreground = colour, Text = $"[{type} - " },
            new Run(Name) { FontWeight = FontWeight.Bold, FontStyle = FontStyle.Italic, Foreground = colour },
            new Run { FontWeight = FontWeight.Bold, Foreground = colour, Text = "]: " }
        };

    private void AddFormattedMessage(ref InlineCollection span, string message, params object?[] propertyValues)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        while (message.Length != 0)
        {
            var inline = new Run { Foreground = GrayBrush };

            var startBracketInt = message.IndexOf('{') + 1;
            var endBracketInt = message.IndexOf('}');
            /*This shows that we are at the end of the message
             or the message has no properties to show*/
            if (startBracketInt == 0 && endBracketInt == -1)
            {
                inline.Text = message;
                span.Add(inline);
                return;
            }

            inline.Text = message[..(startBracketInt - 1)];
            span.Add(inline);
            if (!int.TryParse(message[startBracketInt..endBracketInt], out var number))
            {
                throw new FormatException();
            }

            var valInline = new Run { FontWeight = FontWeight.Bold, Foreground = GetColourBasedOnType(propertyValues[number]) };
            valInline.Text = propertyValues[number]?.ToString() ?? "null";
            span.Add(valInline);
            
            message = message.Substring(endBracketInt + 1, message[(endBracketInt + 1)..].Length);
        }
    }
    
    private static IBrush GetColourBasedOnType(object? o)
    {
        return o switch
        {
            null => Brushes.LightSkyBlue,
            bool => Brushes.LightSkyBlue,
            string => Brushes.Cyan,
            _ when IsNumber(o) => Brushes.Magenta,
            _ => Brushes.Green
        };
    }

    private static bool IsNumber(object o) => o.GetType().GetInterfaces().Any(x => x.Name == typeof(INumber<>).Name);
}
    
public class LoggingPageBuilder : LoggingBuilder
{
    public override ILogging CreateLogger(string name)
    {
        return new LoggingPageLogger(name);
    }
}