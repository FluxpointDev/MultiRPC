using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiRPC.Core
{
    /// <summary>
    /// Page that lives in the sidebar
    /// </summary>
    public interface ISidePage : IRequired
    {
        string IconLocation { get; }

        string LocalizableName { get; }
    }
}
