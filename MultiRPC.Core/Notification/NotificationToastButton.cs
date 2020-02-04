using System;

namespace MultiRPC.Core.Notification
{
    /// <summary>
    /// A button that the <see cref="NotificationToast"/> could have
    /// </summary>
    public class NotificationToastButton
    {
        private NotificationToastButton(string jsonContent, Action<NotificationToast> action)
        {
            JsonContent = jsonContent;
            Action = new Action<NotificationToast>((toast) => 
            {
                action?.Invoke(toast);
                toast.DismissToast();
            });
        }

        /// <summary>
        /// Text to show from it's name that it has in the json language file
        /// </summary>
        public string JsonContent { get; }

        /// <summary>
        /// What should happen when this button gets pressed
        /// </summary>
        public Action<NotificationToast> Action { get; }

        public static NotificationToastButton Create(string jsonContent, Action<NotificationToast> action) => 
            new NotificationToastButton(jsonContent, action);
    }
}
