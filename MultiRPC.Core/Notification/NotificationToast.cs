using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace MultiRPC.Core.Notification
{
    /// <summary>
    /// Notification that the user should see in the client
    /// </summary>
    public class NotificationToast : INotifyPropertyChanged
    {
        static IList<NotificationToast> Notifications = new List<NotificationToast>();
        static uint IDCount = 1;

        private NotificationToast(
            string jsonContent,
            LogEventLevel level,
            bool dismissive,
            TimeSpan timeOut,
            NotificationToastButton[] notificationToastButtons,
            bool giveID = false)
        {
            JsonContent = jsonContent;
            Level = level;
            Dismissive = dismissive;
            TimeOut = timeOut;
            Buttons = notificationToastButtons;
            if (giveID)
            {
                ID = IDCount;
                IDCount++;
            }

            Notifications.Add(this);
        }

        public event EventHandler ToastDismissed;

        private string jsonContent;
        /// <summary>
        /// Text to show from it's name that it has in the json language file
        /// </summary>
        public string JsonContent 
        {
            get => jsonContent;
            set
            {
                if (jsonContent == value)
                {
                    return;
                }
                jsonContent = value;
                OnPropertyChanged();
            }
        }

        private NotificationToastButton[] buttons;
        /// <summary>
        /// Buttons that this toast has
        /// </summary>
        public NotificationToastButton[] Buttons
        {
            get => buttons;
            set
            {
                if (buttons == value)
                {
                    return;
                }
                buttons = value;
                OnPropertyChanged();
            }
        }

        private LogEventLevel level;
        /// <summary>
        /// How severe this should be
        /// </summary>
        public LogEventLevel Level
        {
            get => level;
            set
            {
                if (level == value)
                {
                    return;
                }
                level = value;
                OnPropertyChanged();
            }
        }

        private bool dismissive = true;
        /// <summary>
        /// If this toast can be dismissed
        /// </summary>
        public bool Dismissive
        {
            get => dismissive;
            set
            {
                if (dismissive == value)
                {
                    return;
                }
                dismissive = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan timeOut = default;
        /// <summary>
        /// How long until this toast should go away
        /// </summary>
        public TimeSpan TimeOut
        {
            get => timeOut;
            set
            {
                if (timeOut == value)
                {
                    return;
                }
                timeOut = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Identerfys an notifcation, if not set then when <see cref="DismissToast"/> is called, the notifcation will be lost
        /// </summary>
        public uint ID { get; }

        private bool closed;
        /// <summary>
        /// Gets if the <see cref="NotificationToast"/> is currently closed
        /// </summary>
        public bool Closed
        {
            get => closed;
            private set
            {
                if (closed == value)
                {
                    return;
                }
                closed = value;
                OnPropertyChanged();
            }
        }

        private bool dismissed;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This will make a new <see cref="NotificationToast"/>
        /// </summary>
        /// <param name="jsonContent">Text to show from it's name that it has in the json language file</param>
        /// <param name="level">How severe this should be</param>
        /// <param name="dismissive">If this toast can be dismissed</param>
        /// <param name="timeOut"> How long until this toast should go away</param>
        /// <param name="notificationToastButtons">Buttons that this toast has</param>
        public static NotificationToast Create(
            string jsonContent,
            LogEventLevel level,
            bool dismissive = true,
            TimeSpan timeOut = default,
            bool showNotification = true,
            bool giveID = false,
            params NotificationToastButton[] notificationToastButtons)
        {
            //This notification doesn't need to be made/shown
            if (!NotificationCenter.Logger.IsEnabled(level))
            {
                return null;
            }

            var toast = new NotificationToast(jsonContent, level, dismissive, timeOut, notificationToastButtons, giveID);
            var content = LanguagePicker.GetLineFromLanguageFile(jsonContent);
            switch (level)
            {
                case LogEventLevel.Verbose:
                    NotificationCenter.Logger.Verbose(content);
                        break;
                case LogEventLevel.Debug:
                    NotificationCenter.Logger.Debug(content);
                    break;
                case LogEventLevel.Information:
                    NotificationCenter.Logger.Information(content);
                    break;
                case LogEventLevel.Warning:
                    NotificationCenter.Logger.Warning(content);
                    break;
                case LogEventLevel.Error:
                    NotificationCenter.Logger.Error(content);
                    break;
                case LogEventLevel.Fatal:
                    NotificationCenter.Logger.Fatal(content);
                    break;
                default:
                    NotificationCenter.Logger.Information(content);
                    break;
            }
            toast.Closed = !showNotification;
            NotificationCenter.NewNotificationToastPush(toast);

            return toast;
        }

        public static NotificationToast Get(uint id) =>
            Notifications.First(x => x.ID == id);

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Makes <see cref="Closed"/> false, intercating that the toast should be reopened
        /// </summary>
        public void OpenToast() 
        {
            this.Closed = false;
            dismissed = false;
        }

        /// <summary>
        /// Fires <see cref="ToastDismissed"/>, allowing the user to close a toast.
        /// </summary>
        public void DismissToast()
        {
            if (dismissed)
            {
                return;
            }

            ToastDismissed?.Invoke(this, EventArgs.Empty);
            dismissed = true;

            if (this.ID == 0)
            {
                Notifications.Remove(this);
            }
            this.Closed = true;
        }
    }
}
