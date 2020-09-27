using MultiRPC.Core.Rpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiRPC.Core
{
    public interface IRpcPage
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
    }
}
