using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Pages
{
    public partial class LoggingPage : SidePage
    {
        public LoggingPage() { }

        public override string IconLocation => "Icons/Logging";
        public override string LocalizableName => "Log";
        public override void Initialize(bool loadXaml) => InitializeComponent(loadXaml);
    }
    
    public class LoggingPageLogger : ILogging
    {
        public LoggingPageLogger(string name) => Name = name;
        
        public void Debug(string message, params object?[] propertyValues)
        {
            throw new NotImplementedException();
        }

        public void Information(string message, params object?[] propertyValues)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message, params object?[] propertyValues)
        {
            throw new NotImplementedException();
        }

        public void Error(string message, params object?[] propertyValues)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception e, params object?[] propertyValues)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }
        public LogLevel? LogLevel { get; set; }
    }

    public class LoggingPageBuilder : LoggingBuilder
    {
        public override ILogging CreateLogger(string name)
        {
            return new LoggingPageLogger(name);
        }
    }
}