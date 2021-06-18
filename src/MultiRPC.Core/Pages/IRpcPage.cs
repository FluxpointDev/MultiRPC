using System;
using System.ComponentModel;
using MultiRPC.Core.Rpc;

namespace MultiRPC.Core.Pages
{
    public interface IRpcPage : INotifyPropertyChanged
    {
        /// <summary>
        /// The rich presence made from the contents of this page
        /// </summary>
        RichPresence RichPresence { get; }

        /// <summary>
        /// Gets when this page is being accessed
        /// </summary>
        event EventHandler Accessed;

        /// <summary>
        /// The localizable name of this page 
        /// </summary>
        public string LocalizableName { get; }

        /// <summary>
        /// If the rich presence is vaild for use for a RPC Client
        /// </summary>
        public bool VaildRichPresence { get; }
    }
}
