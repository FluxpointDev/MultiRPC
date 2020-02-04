using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Core.Notification
{
    /// <summary>
    /// Arguments for when a <see cref="NotificationCenter"/> changes
    /// </summary>
    class NotificationToastArgs : EventArgs
    {
        /// <summary>
        /// Notification to trigger this
        /// </summary>
        NotificationToast Toast { get; }

        /// <summary>
        /// If they want us to show the <see cref="NotificationToast"/>
        /// </summary>
        bool Show { get; }
    }
}
